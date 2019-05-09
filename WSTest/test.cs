using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWebSocket.RR;
using JSONParserLibrary;

namespace WSTest
{
    class test
    {
        static void Main(string[] args)
        {
            testJsonParser();
            //testWS();
            Console.ReadKey();
        }

        public static void testJsonParser() {
            JSONParser pars = new JSONParser("{\"cmd\": \"ajsdfj\", \"data\":{\"c\":234.56}}");
            Console.WriteLine(pars["data.c"].GetValue<float>());
        }

        public static async void testWS() {
            RRServer server = new RRServer("ws://0.0.0.0:20001", (string arg1, IRRClient arg2) =>
            {
                Console.WriteLine("server {0}", arg1);
                arg2.SendMessageAsync("hello client");
                arg2.Close();
            }, (IRRClient obj) =>
            {
                Console.WriteLine("connected ip: {0}, port {1}", obj.address, obj.port);
            });

            RRClient client = new RRClient("ws://127.0.0.1:20001");
            Console.WriteLine("closed {0}", client.IsClosed());
            string res = await client.SendMessageAsync("hello server");
            Console.WriteLine("client {0} is closed {1}", res, client.IsClosed());
            //client.Close();
        }
    }
}
