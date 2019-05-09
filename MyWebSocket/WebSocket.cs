/*
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
0xB обозначает открытие соединения !!!
0xC-F зарезервированы для будущих управляющих фреймов.
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

namespace MyWebSocket
{
	public abstract class WebSocket : Connect
	{
		private TcpClient _client;
		private NetworkStream stream;
		private object locker = new object();

		private static Action<int, NetworkStream, WebSocket>[] opcodeReact = new Action<int, NetworkStream, WebSocket>[12];
		private readonly static byte finMask = 0x80;
		private readonly static byte opcodeMask = 0x0F;
		private readonly static byte smallLenght = 0x7F;

		static WebSocket()
		{
			opcodeReact[1] = (fin, stream, WS) =>
			{//приём текстового сообщения
				byte[] second = new byte[1];
				stream.Read(second, 0, 1);
				int is_mask = (second[0] & finMask) >> 7;
				UInt64 lenght = (UInt64)(second[0] & smallLenght);
				byte[] buffer = null;
				byte[] mask = null;

				if (lenght == 126)
				{
					byte[] lenght2 = new byte[2];
					stream.Read(lenght2, 0, 2);
					lenght = BitConverter.ToUInt16(lenght2, 0);
					buffer = new byte[1024];
				}
				else if (lenght == 127)
				{
					byte[] lenght8 = new byte[8];
					stream.Read(lenght8, 0, 8);
					lenght = BitConverter.ToUInt64(lenght8, 0);
					buffer = new byte[2048];
				}
				else
				{
					buffer = new byte[lenght];
				}

				if (is_mask == 1)
				{
					mask = new byte[4];
					stream.Read(mask, 0, 4);
				}


				StringBuilder sb = new StringBuilder();
				do
				{
					int count = stream.Read(buffer, 0, buffer.Length);
					sb.Append(Encoding.UTF8.GetString(buffer, 0, count));
					lenght -= (ulong)count;
				} while (lenght > 0);

				ThreadPool.QueueUserWorkItem((state) =>
				{
					//if (WS._observer == null) { return; }
					//WS._observer.OnMessage(sb.ToString());
					WS.OnMessage(sb.ToString());
				});
			};
			opcodeReact[8] = (fin, stream, WS) => {//приём сообщения о закрытии соединения
				WS._client.Close();
			};
		}

		public WebSocket(string url) : base(url)
		{
			_client = new TcpClient();
			_client.Connect(address, port);
			create(_client);
		}
		internal WebSocket(TcpClient client, bool encryption) : base(client, encryption)
		{
			_client = client;
			create(client);
		}

		public virtual void SendMessage(String message)
		{//отправка текстового сообщения
			List<byte> two_byte = new List<byte>();
			two_byte.Add(0x81);
			byte[] buffer = Encoding.UTF8.GetBytes(message);
			if (buffer.Length >= 126)
			{
				if (buffer.Length <= UInt16.MaxValue)
				{
					two_byte.Add((byte)(0x7E | mask));
					UInt16 count = Convert.ToUInt16(buffer.Length);
					byte[] count_res = BitConverter.GetBytes(count);
					two_byte.AddRange(count_res);
				}
				else
				{
					two_byte.Add((byte)(0x7F | mask));
					UInt64 count = Convert.ToUInt64(buffer.Length);
					byte[] count_res = BitConverter.GetBytes(count);
					two_byte.AddRange(count_res);
				}
			}
			else
			{
				two_byte.Add((byte)(Convert.ToByte(buffer.Length) | mask));
			}

			WriteToBufferAtomic((stream) =>
			{
				stream.Write(two_byte.ToArray(), 0, two_byte.Count);
				stream.Write(buffer, 0, buffer.Length);
			});
		}

		public void Close()
		{//закрытие соединения (отправка сообщения о закрытии другой стороне)
			byte[] buffer = new byte[1] { 136 };
			WriteToBufferAtomic((NetworkStream obj) =>
			{
				obj.Write(buffer, 0, 1);
			});
		}
		public bool IsClosed() {
			return !_client.Connected;
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
			lock (locker) {
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

		private void create(TcpClient client) {
			stream = client.GetStream();
			OnOpen();

			ThreadPool.QueueUserWorkItem((state) => {
				byte[] data = new byte[1];
				try
				{
					while (true)
					{//код принимающий данные из сети и решающий кто их будет обрабатывать зависит от опкода
						if (!stream.CanRead) { break; }
						int count = stream.Read(data, 0, 1);
						if (count == 0) { break; }
						int fin = (data[0] & finMask) >> 7;
						int opcode = data[0] & opcodeMask;
						opcodeReact[opcode](fin, stream, this);
					}
				}
				catch (ThreadAbortException err) {}
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
		}
	}
}
