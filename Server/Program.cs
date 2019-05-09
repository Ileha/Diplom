using System;
using MyWebSocket.RR;
using JSONParserLibrary;
using System.IO;
using MyRandom;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using CommadInterfaces;
using IOTServer.StatisticData;
using System.Threading.Tasks;
using IOTServer.Commands;

namespace IOTServer
{
	class MainClass
	{
		const ushort BROADCAST_UDP_SENDER_PORT = 20042;
		const ushort UDP_STATISTICS_SERVER_PORT = 20043;
		const int UDP_BROADCAST_TIMEOUT_MS = 2000;
		const string CONFIG_PATH = "./config.json";

        private static CommonData dataHolder;
		private static BroadcastUDPClient flagSender;
		private static RRServer server;
		private static CommandArray<ClientData> commands;
		private static UDPStatisticsServer statisticsServer;

		public static JSONParser config { get; private set; }

		public static void Main(string[] args) {
			config = JSONParser.ReadConfig(CONFIG_PATH);
            dataHolder = new CommonData();

			ConfigureCommands();
			ConfigureRRServer();
			CreateStatisticsServer();
			CreateFlagSender();

			Console.ReadKey();
			flagSender.Dispose();
			server.Close();
			statisticsServer.Dispose();
		}

		/*
		* сознаие udp сервера для сбора статистики
 		*/
		private static void CreateStatisticsServer()
		{
			
			statisticsServer = new UDPStatisticsServer(UDP_STATISTICS_SERVER_PORT, (ip, data) =>
			{
				/*
				0                   1                   2                   3
				0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
				+---------------------------------------------------------------+
				|                           session id                          |
				|                          Data 508 bytes                       |
				*/
				uint sessionID = data.ReadUInt32();
				//Console.WriteLine(sessionID);
                dataHolder.statisticData.AddData(ip.Address, data.BaseStream.Length, sessionID);
			});
		}

		/*
		* создаие udp клиента который посылает в broadcast информацию о сервере
		*/
		private static void CreateFlagSender() {
			UInt16 port = config["server_port"].GetValue<ushort>();
			uint id = MyRandom.MyRandom.GetRandomUInt32();
			MemoryStream sendData = new MemoryStream();

			using (BinaryWriter writer = new BinaryWriter(sendData)) {
				writer.Write(port);
				writer.Write(id);
			}
			flagSender = new BroadcastUDPClient(UDP_BROADCAST_TIMEOUT_MS, 
			                                    BROADCAST_UDP_SENDER_PORT, 
			                                    config["network.address"].GetValue<string>(),
                                                config["network.mask"].GetValue<string>(), 
			                                    sendData.ToArray());
		}

		/*
		* создание websocket сервера
		*/
		private static void ConfigureRRServer() {
			//requ	{cmd, data: {}}
			//resp	{ok: {message}}
			//error	{error: {message}}
            server = new RRServer(String.Format("ws://0.0.0.0:{0}", config["server_port"].GetValue<UInt16>()), (string arg1, IRRClient arg2) =>
			{
				Console.WriteLine(arg1);
				JSONParser inputData = null;
				try {
					inputData = new JSONParser(arg1);
				}
				catch (Exception err) {
					arg2.SendMessageAsync("{\"error\": {\"message\": \"bad JSON data\"}}");
					return;
				}

                commands.Execute(inputData["cmd"].GetValue<string>(), new ClientData(arg2, new JSONParser(inputData["data"])));
			}, (IRRClient obj) => {
				Console.WriteLine("connected ip: {0}, port {1}", obj.address, obj.port);
			});
		}

		/*
		 * создание управляющих сервером команд
		*/
		private static void ConfigureCommands() {
			commands = new CommandArray<ClientData>();
			commands.AddCommand((c) => {//добавляет клиента и записывает как его найти
                IServerCommand cmd = new CommandAddClient(dataHolder);
				c.Name = "AddClient";
				c.Execute = (ClientData[] arguments) => {
                    cmd.Execute(arguments[0]);
				};
			});
			commands.AddCommand((c) => {//фильтрует клиентов по имени и группе
                IServerCommand cmd = new CommandClients(dataHolder);
				c.Name = "clients";
				c.Execute = (ClientData[] arguments) => {
                    cmd.Execute(arguments[0]);
				};
			});
			commands.AddCommand((c) => {
                IServerCommand cmd = new CommandStressTest(dataHolder);
				c.Name = "stress";
				c.Execute = (ClientData[] arguments) => {
                    cmd.Execute(arguments[0]);
				};
			});
		}
	}

}
