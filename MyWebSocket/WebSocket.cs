﻿/*
 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-------+-+-------------+-------------------------------+
|F|R|R|R| опкод |М| Длина тела  |    Расширенная длина тела     |
|I|S|S|S|(4бита)|А|   (7бит)    |            (1 байт)           |
|N|V|V|V|       |С|             |(если длина тела==126 или 127) |
| |1|2|3|       |К|             |                               |
| | | | |       |А|             |                               |
+-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
|  Продолжение расширенной длины тела, если длина тела = 127    |
+ - - - - - - - - - - - - - - - +-------------------------------+
|                               |  Ключ маски, если МАСКА = 1   |
+-------------------------------+-------------------------------+
| Ключ маски (продолжение)      |       Данные фрейма ("тело")  |
+-------------------------------- - - - - - - - - - - - - - - - +
:                     Данные продолжаются ...                   :
+ - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
|                     Данные продолжаются ...                   |
+---------------------------------------------------------------+

FIN: 1 бит
Одно сообщение, если оно очень длинное (вызовом send можно передать хоть целый файл),
может состоять из множества фреймов («быть фрагментированным»).
У всех фреймов, кроме последнего, этот фрагмент установлен в 0, у последнего – в 1.
Если сообщение состоит из одного-единственного фрейма, то FIN в нём равен 1.

Опкод: 4 бита
Задаёт тип фрейма, который позволяет интерпретировать находящиеся в нём данные. Возможные значения:

0x1 обозначает текстовый фрейм.
0x2 обозначает двоичный фрейм.
0x3-7 зарезервированы для будущих фреймов с данными.
0x8 обозначает закрытие соединения этим фреймом.
0x9 обозначает PING.
0xA обозначает PONG.
0xB-F зарезервированы для будущих управляющих фреймов.
0x0 обозначает фрейм-продолжение для фрагментированного сообщения. Он интерпретируется, исходя из ближайшего предыдущего ненулевого типа.

*/

/*
fin/mask mask: 10000000 = 0x80
opcode mask: 00001111   = 0x0F
lenght mask: 01111111   = 0x7F

//open connect 139
0       1       2       3
10001011

//close connect 136
0       1       2       3
10001000

//ping 137
0       1       2       3
10001001

*/

using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MyWebSocket
{
	public abstract class WebSocket : Connect
	{
        private const byte FIN_MASK = 0x80;
        private const byte OPCODE_MASK = 0x0F;
        private const byte START_LENGHT = 0x7F;

        private static MyWebSocket.ThreadPool TPool = new MyWebSocket.ThreadPool((Environment.ProcessorCount*2)+1);
        private static Action<int, WebSocket>[] opcodeReact = new Action<int, WebSocket>[12];

		private TcpClient client;
        private NetworkStream stream { get { return client.GetStream(); } }
		private object writeLocker = new object();
        private Thread task;

        private bool justStart = false;

		static WebSocket()
		{
			opcodeReact[1] = (fin, WS) =>
			{//приём текстового сообщения
                //Console.WriteLine("text message");
                UInt64 lenght = 0;
                byte[] buffer = null;
                byte[] mask = null;

                BinaryReader reader = new BinaryReader(WS.stream);
                byte second = reader.ReadByte();

                int is_mask = (second & FIN_MASK) >> 7;
                lenght = (UInt64)(second & START_LENGHT);

                if (lenght == 126) {
                    lenght = reader.ReadUInt16();
                    buffer = new byte[1024];
                }
                else if (lenght == 127) {
                    lenght = reader.ReadUInt64();
                    buffer = new byte[2048];
                }
                else {
                    buffer = new byte[lenght];
                }

                if (is_mask == 1) {
                    mask = reader.ReadBytes(4);
                }
                

				StringBuilder sb = new StringBuilder();

                ulong redBytes = 0;
                while (redBytes < lenght) {
                    int needToRead = Math.Min(buffer.Length, (int)(lenght - redBytes));
                    int count = WS.stream.Read(buffer, 0, needToRead);
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, count));
                    redBytes += (ulong)count;
                }

                System.Threading.ThreadPool.QueueUserWorkItem((state) => { WS.OnMessage(sb.ToString()); });
                
			};
			opcodeReact[8] = (fin, WS) => {//приём сообщения о закрытии соединения
                //Console.WriteLine("closed message");
                WS.task.Abort();
                WS.client.Close();
			};
		}

		public WebSocket(string url) : base(url)
		{
			client = new TcpClient();
			client.Connect(address, port);
		}
		internal WebSocket(TcpClient client, bool encryption) : base(client, encryption)
		{
			this.client = client;
		}

		public virtual void SendMessage(String message) {//отправка текстового сообщения
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            MemoryStream header = new MemoryStream(2);
            using (BinaryWriter writer = new BinaryWriter(header)) {
                writer.Write((byte)0x81);
                if (buffer.Length >= 126) {
                    if (buffer.Length <= UInt16.MaxValue) {
                        writer.Write((byte)(0x7E | mask));
                        UInt16 count = Convert.ToUInt16(buffer.Length);
                        writer.Write(count);
                    }
                    else {
                        writer.Write((byte)(0x7F | mask));
                        UInt64 count = Convert.ToUInt64(buffer.Length);
                        writer.Write(count);
                    }
                }
                else {
                    writer.Write((byte)(Convert.ToByte(buffer.Length) | mask));
                }

                header.Seek(0, SeekOrigin.Begin);

                WriteToBufferAtomic((stream) => {
                    header.CopyTo(stream, (int)header.Length);
                    stream.Write(buffer, 0, buffer.Length);
                });
            }
		}

		public void Close()
		{//закрытие соединения (отправка сообщения о закрытии другой стороне)
			byte[] buffer = new byte[1] { 136 };
			WriteToBufferAtomic((NetworkStream obj) =>
			{
				obj.Write(buffer, 0, 1);
                task.Abort();
                client.Close();
			});
		}
		public bool IsClosed() {
			return !client.Connected;
		}

		/*
		* переопределяемые функции
		*/
		protected abstract void OnMessage(String message);
		protected abstract void OnError(Exception err);
		protected abstract void OnOpen();
		protected abstract void OnClose();

		/*
		* атомарная операция записи в сеть
		*/
		private void WriteToBufferAtomic(Action<NetworkStream> operations) {
			lock (writeLocker) {
				operations(stream);
			}
		}

		/*
		* возвращает маску в зависимости от наличия "шифрования"
		*/
		private byte mask {
			get {
				if (isEncrypt) {
					return (byte)128;
				}
				else {
					return (byte)0;
				}
			}
		}

		protected void Start() {
            if (justStart) { return; }

            justStart = true;

			OnOpen();

            //int messageCount = 0;
            //Console.WriteLine("create WS");

            //client.Client.ReceiveTimeout = 1000;

            task = new Thread(() =>
            {
				byte[] data = new byte[1];
                int count;
                try {
                    while (true)
                    {//код принимающий данные из сети и решающий кто их будет обрабатывать зависит от опкода
                        count = stream.Read(data, 0, 1);
                        int fin = (data[0] & FIN_MASK) >> 7;
                        int opcode = data[0] & OPCODE_MASK;
                        //Console.WriteLine("first: {0} count: {1}", data[0], messageCount++);
                        opcodeReact[opcode](fin, this);
                    }
                    
                }
                catch (ThreadAbortException abrot) {
                    Console.WriteLine("abrot");
                }
                catch (Exception err)
                {
                    OnError(err);
                }
				finally {
                    if (client.Connected) {
                        client.Close();
                    }
					OnClose();
				}
			});
            task.Start();
		}
	}
}
