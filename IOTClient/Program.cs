using System;
using System.Collections.Generic;
using System.Net;
using JSONParserLibrary;
using MyWebSocket.RR;
using CommadInterfaces;
using System.Net.Sockets;
using System.Text;
using MyRandom;
using System.IO;
using IOTClient.Commands;

namespace IOTClient
{
	class MainClass
	{
        public const ushort UDP_STATISTICS_SERVER_PORT = 20043;
		const ushort BROADCAST_UDP_SENDER_PORT = 20042;
		const string CONFIG_PATH = "./config.json";

		private static Dictionary<IPEndPoint, UInt32> knownServers = new Dictionary<IPEndPoint, uint>();

		private static FlagReceiver receiver;
		private static RRServer rRServer;
		private static CommandArray<ClientData> commands;

		public static string hardwareID { get; private set; }
		public static IPart information { get; private set; }
		public static JSONParser config { get; private set; }

		public static void Main(string[] args)
		{
			config = JSONParser.ReadConfig(CONFIG_PATH); //чтение файла конфигурации с диска
			hardwareID = Hardware.GetID(); //идентификатор оборудования строися на основе mac адресов
			information = new PartStruct() //информация о клиенте отсылается серверу
				.Add("name", config["name"])
				.Add("groupe", config["group"])
				.Add("id", hardwareID)
				.Add("port", config["serverPort"]);
			
			receiver = ConfigureReceiver();
			commands = ConfigureCommands();
			rRServer = ConfigureServer();

			Console.ReadKey();
			//освобождение ресурсов и завершение программы
			receiver.Dispose();
			rRServer.Dispose();
		}

		/*
		 * конфигурация команд клиента
		*/
		private static CommandArray<ClientData> ConfigureCommands() {
			CommandArray<ClientData> res = new CommandArray<ClientData>();
			res.AddCommand((c) => {
                ICommand cmd = new CommandStress();
				c.Name = "stress";
				c.Execute = (ClientData[] arguments) => {
                    cmd.Execute(arguments[0]);
				};
			});
            res.AddCommand((c) => {
                ICommand cmd = new CommandLoad();
                c.Name = "load";
                c.Execute = (ClientData[] arguments) => {
                    cmd.Execute(arguments[0]);
                };
            });
            res.AddCommand((c) => {
                ICommand cmd = new CommandPerformance();
                c.Name = "performance";
                c.Execute = (ClientData[] arguments) => {
                    cmd.Execute(arguments[0]);
                };
            });

			return res;
		}

		/*
		* тестовая функция для выполнения команд ОС
		*/
		static async void testCMD() {
			try
			{
				string res = await BashExecutor.ExecuteBashCommand("cd /dsfghdg");
				Console.WriteLine("result: {0}", res);
			}
			catch (Exception err) {
				Console.WriteLine("error: {0}", err);
			}
		}


		/*
		 * создание сервера принимающего команды с управляющего сервера
		*/
		static RRServer ConfigureServer() {
			//requ	{cmd, data: {}}
			//resp	{ok: {message}}
			//error	{error: {message}}

			RRServer server = new RRServer(String.Format("ws://0.0.0.0:{0}", config["serverPort"].GetValue<UInt16>()));
            server.onMessage += (string arg1, IRRClient arg2) => {
				JSONParser inputData = null;
				try {
					inputData = new JSONParser(arg1);
				}
				catch (Exception err) {
					arg2.SendMessageAsync("{\"error\": {\"message\": \"bad JSON data\"}}");
					return;
				}
                Console.WriteLine(inputData.ToJSON());

				commands.Execute(inputData["cmd"].GetValue<string>(), new ClientData(arg2, new JSONParser(inputData["data"])));
			};

            return server;
		}

		/*
		 * создание сервера слушающего broadcast и передающего информацию о себе управляющему серверу
		*/
		static FlagReceiver ConfigureReceiver() {
			return new FlagReceiver(BROADCAST_UDP_SENDER_PORT, async (IPEndPoint arg1, byte[] arg2) => {
				UInt32 id = BitConverter.ToUInt32(arg2, 2);
                lock (knownServers) {
                    if (knownServers.ContainsKey(arg1)) {
                        if (knownServers[arg1] == id) {
                            return;
                        }
                        else {
                            knownServers[arg1] = id;
                        }
                    }
                    else {
                        knownServers.Add(arg1, id);
                    }
                }

				UInt16 port = BitConverter.ToUInt16(arg2, 0);
				Console.WriteLine("server on {0}:{1} id: {2}", arg1.Address, port, id);
				RRClient client = new RRClient(String.Format("ws://{0}:{1}", arg1.Address, port));
				string data = await client.SendMessageAsync(new PartStruct()
				                              .Add("cmd", "AddClient")
				                              .Add("data", information).ToJSON());
				client.Close();
			});
		}
	}
}
