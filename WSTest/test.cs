using System;
using MyWebSocket.RR;
using System.Threading;

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
            testWS();

            //RRClient[] clients = new RRClient[Convert.ToInt32(args[0])];
            //int messageCount = Convert.ToInt32(args[1]);
            //int delay = Convert.ToInt32(args[2]);

            //for (int i = 0; i < clients.Length; i++)
            //{
            //    clients[i] = new RRClient("ws://10.0.1.2:20001");
            //}

            //for (int i = 0; i < messageCount; i++)
            //{
            //    int index = i % clients.Length;
            //    openWS(clients[index], String.Format("hello server from connect {0}\t message {1}", index, i));
            //    Thread.Sleep(delay);
            //}

            //Console.ReadLine();
            //Console.WriteLine(count);

            //for (int i = 0; i < clients.Length; i++)
            //{
            //    clients[i].Close();
            //}
        }

        public static void testWS() {
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
