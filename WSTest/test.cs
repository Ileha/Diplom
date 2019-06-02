using System;
using MyWebSocket.RR;
using System.Threading;
using MyWebSocket.RawWs;
using MyRandom;
using System.Diagnostics;
using JSONParserLibrary;

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
            
            //testWSServer();
            //testWSclients(1, 50, 0);
            //testWSclients(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));

            //RandomTest();
            //DelayTest();

            //double[] array = new double[] { 2, 3, 7, 9, 1, 8, 4, 5 };
            //BubbleSort(array);
            //Console.WriteLine(array);

            JSONTest();

            Console.ReadKey();
        }
        private static void JSONTest() {
            JSONParser pars = new JSONParser(new PartStruct()
              .Add("firstName", "Иван")
              .Add("lastName", "Иванов")
              .Add("address", new PartStruct()
                .Add("streetAddress", "Московское ш., 101, кв.101")
                .Add("city", "Ленинград")
                .Add("postalCode", 101101)
                .Add("id", 3.45d)
              )
              .Add("phoneNumbers", new PartArray()
                .Add("812 123-1234")
                .Add("916 123-4567")
              )
            );
            //Console.WriteLine(pars.ToJSON());
            //Console.WriteLine(pars["address.id"].GetValue<double>());

            Console.WriteLine(pars["phoneNumbers.1"].GetValue<String>());
            Console.WriteLine(pars.Data.Get("address").Get("postalCode").GetValue<int>());
            Console.WriteLine(pars.Data.ByPath("phoneNumbers.0").GetValue<String>());

            IPart streetAddress = null;
            Console.WriteLine(pars.Data.ByPathSave("address.streetAddress", out streetAddress));
            Console.WriteLine(streetAddress.GetValue<string>());

            IPart firstName = null;
            Console.WriteLine(pars.Data.ByPathSave("name.firstName", out streetAddress));//wrong request
            Console.WriteLine(firstName == null);
        }

        private static void BubbleSort(double[] array)
        {
            double temp;
            for (int i = 0; i < array.Length; i++) {
                for (int j = 1; j < array.Length-i; j++) {
                    if (array[j-1] > array[j]) {
                        temp = array[j-1];
                        array[j-1] = array[j];
                        array[j] = temp;
                    }
                }
            }

            //double temp;
            //for (int i = 0; i < array.Length; i++)
            //{
            //    for (int j = i + 1; j < array.Length; j++)
            //    {
            //        if (array[i] > array[j])
            //        {
            //            temp = array[i];
            //            array[i] = array[j];
            //            array[j] = temp;
            //        }
            //    }
            //}
        }

        public static void DelayTest() {
            double ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000d;
            long last = 0;
            long[] delays = new long[] { 500, 40000, 30000, 1000, 500, 50000 }; //в микросекундах

            Stopwatch sw;
            sw = Stopwatch.StartNew();

            for (int i = 0; i < delays.Length; i++) {
                last += (long)(delays[i] * ticksPerMicrosecond);
                while (sw.Elapsed.Ticks < last) {
                    
                }
                Console.WriteLine((double)sw.Elapsed.Ticks / (double)TimeSpan.TicksPerMillisecond);
            }  

            sw.Stop();
        }

        public static void RandomTest() {
            int rCount = 1000;
            //double[] randomHolder = new double[rCount];

            for (int i = 0; i < rCount; i++) {
                Console.WriteLine(MyRandom.MyRandom.ErlangDistribution(1, 2));
            }
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
                clients[i].onClose += (RawSocket) => { Console.WriteLine("client close"); };
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
            RRServer server = new RRServer("ws://0.0.0.0:20001");
            server.onMessage += (string arg1, IRRClient arg2) => {
                Console.WriteLine("server {0}", arg1);
                arg2.SendMessageAsync(arg1);
            };
            server.onOpen += (IRRClient obj) =>
            {
                Console.WriteLine("connected ip: {0}, port {1}", obj.address, obj.port);
                obj.onClose += (RawSocket) => { Console.WriteLine("server side close"); };
            };

            
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
