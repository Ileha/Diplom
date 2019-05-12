using System;
using MyWebSocket.RR;
using System.Threading;
using MyWebSocket.RawWs;

namespace WSTest
{
    class test
    {

        static int count = 0;
        static object locker = new object();
        static void add()
        {
            lock (locker)
            {
                count++;
            }
        }

        static void Main(string[] args)
        {
            //testRawServer();
            //testRawclients(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));

            testWSServer();
            //testWSclients(1, 50, 0);
            //testWSclients(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
        }

        public static void testRawServer() {
            RawServer server = new RawServer("ws://0.0.0.0:20001");
            server.onMessage += (string arg1, RawSocket arg2) =>
            {
                Console.WriteLine("server {0}", arg1);
                arg2.SendMessage(arg1);
            };

            server.onOpen += (RawSocket obj) =>
            {
                obj.onClose += (RawSocket) => { Console.WriteLine("server side close"); };
                Console.WriteLine("connected ip: {0}, port {1}", obj.address, obj.port);
            };
        }

        public static void testRawclients(int connectCount, int messageCount, int delay)
        {
            RawSocket[] clients = new RawSocket[connectCount];
            for (int i = 0; i < clients.Length; i++) {
                clients[i] = new RawSocket("ws://10.0.1.2:20001");
                clients[i].onMessage += (string message, RawSocket client) => {
                    Console.WriteLine("client {0}", message);
                    add();
                };
                clients[i].onClose += (RawSocket) => { Console.WriteLine("client close"); };
            }

            for (int i = 0; i < messageCount; i++)
            {
                int g = i;
                int index = i % clients.Length;
                ThreadPool.QueueUserWorkItem((state) => {
                    clients[index].SendMessage(String.Format("hello server from connect {0}\t message {1}", index, g));
                });
                Thread.Sleep(delay);
            }

            Console.ReadLine();
            Console.WriteLine(count);

            for (int i = 0; i < clients.Length; i++)
            {
                clients[i].Close();
            }
        }


        public static void testWSclients(int connectCount, int messageCount, int delay) {
            RRClient[] clients = new RRClient[connectCount];

            for (int i = 0; i < clients.Length; i++)
            {
                clients[i] = new RRClient("ws://10.0.1.2:20001");
            }

            for (int i = 0; i < messageCount; i++)
            {
                int index = i % clients.Length;
                openWS(clients[index], String.Format("hello server from connect {0}\t message {1}", index, i));
                Thread.Sleep(delay);
            }

            Console.ReadLine();
            Console.WriteLine(count);

            for (int i = 0; i < clients.Length; i++)
            {
                clients[i].Close();
            }
        }

        public static void testWSServer() {
            RRServer server = new RRServer("ws://0.0.0.0:20001", (string arg1, IRRClient arg2) =>
            {
                Console.WriteLine("server {0}", arg1);
                arg2.SendMessageAsync(arg1);
                //arg2.Close();
            }, (IRRClient obj) =>
            {
                Console.WriteLine("connected ip: {0}, port {1}", obj.address, obj.port);
            });

            
        }

        public static async void openWS(RRClient client, string data)
        {

            string res = await client.SendMessageAsync(data);
            Console.WriteLine("client {0}", res);
            add();
            //client.Close();
        }
    }
}
