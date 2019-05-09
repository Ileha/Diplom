using System;
using CommadInterfaces;
using MyWebSocket.RR;
using JSONParserLibrary;
using System.Collections.Generic;
using System.Threading;

namespace UiserClient
{
	class MainClass
	{
		private static bool execute = true;
		private static CommandArray<CommandData> array = new CommandArray<CommandData>();
		private static CommandArray<CommandData> helpArray = new CommandArray<CommandData>();
		private static RRClient client;
		private static List<Client> selected = new List<Client>();
		private static Thread mainThread;

		public static void Main(string[] args) //ip, port
		{
			//client = new RRClient(String.Format("ws://{0}:{1}", args[0], args[1]));
            client = new RRClient(String.Format("ws://{0}:{1}", "127.0.0.1", "20146"));
			mainThread = Thread.CurrentThread;

			ConfigHelpCommands();
			ConfigMainCommands();

			client.AddOnCloseObserver((IRRClient mainclient) => {//при падении сервера програма завершает работу
				if (execute) {
					Console.WriteLine("server is droped");
					array.Execute("exit", null);
					mainThread.Abort();
				}
			});

			//cmd [ARGS] [OPTIONS]
			try
			{
				while (execute)
				{
					Console.WriteLine("Enter command use cmd [ARGS] [OPTIONS]");
					string cmd = Console.ReadLine();
					CommandData splited;
					try {
						splited = new CommandData(cmd);
					}
					catch (BadInputException) {
						Console.WriteLine("Bad input");
						continue;
					}

					try {
						array.Execute(splited.Cmd, splited);
					}
					catch (Exception err) {
						Console.WriteLine(err);
					}
				}
			}
			catch (ThreadAbortException err) {
				
			}
		}

		private static PartArray getSelectedClients() {
			PartArray res = new PartArray();
			foreach (Client iotClient in selected) {
				res.Add(iotClient.HID);
			}
			return res;
		}

		private static void ConfigMainCommands() {
			array.AddCommand(c => {//выводит список выбранных клиентов
			    c.Name = "current";
			    c.Execute = (CommandData[] arguments) => {
					foreach (Client cli in selected) {
						Console.WriteLine(cli);
					}
				};
			});
			array.AddCommand(c => {//запрашивает у сервера клинтов по имени и или группе и выбирает их
			    c.Name = "clients";
				CommandDataPattern pattern = new CommandDataPattern()
					.AddOption('n', true)
					.AddOption('g', true);
			    c.Execute = async(CommandData[] arguments) => {
					//{cmd:clients, data:{name, groupe}}
					CommandData arg = arguments[0];
					arg.SetPattern(pattern);

					IPart reqData = new PartStruct();
					IPart req = new PartStruct()
						.Add("cmd", "clients")
						.Add("data", reqData);
					if (arg.IsKey('n')) {
						reqData.Add("name", arg.GetKeyValue('n'));
					}
					if (arg.IsKey('g')) {
						reqData.Add("groupe", arg.GetKeyValue('g'));
					}

					JSONParser data = new JSONParser(await client.SendMessageAsync(req.ToJSON()));
					selected.Clear();

					Console.WriteLine("{0} client(s) have found", data.Data.Count);
					foreach (IPart part in data.Data) {
						selected.Add(new Client(part.ByPath("address").GetValue<string>(), 
						                        part.ByPath("name").GetValue<string>(),
                                                part.ByPath("groupe").GetValue<string>(),
                                                part.ByPath("id").GetValue<string>()
						                       ));
					}
				};
			});
			array.AddCommand(c => {//завершает работу програмы
				c.Name = "exit";
				c.Execute = (CommandData[] arguments) => {
					if (!client.IsClosed()) {
						client.Close();
					}
					execute = false; 
				};
			});
			array.AddCommand(c => {//записывает в консоль руководство по команде
				c.Name = "help";
				c.Execute = (CommandData[] arguments) => {
					CommandData arg = arguments[0];
					arg.SetPattern(CommandDataPattern.EmptyPattern);
					helpArray.Execute(arg.args[0], arguments);
				};
			});

			array.AddCommand(c => {
				CommandDataPattern pattern = new CommandDataPattern()
					.AddOption('c', true);
				//{cmd: StressTest data: {dataCount, clients:[]}}
				c.Name = "stress";
				c.Execute = async (CommandData[] arguments) => {
					CommandData arg = arguments[0];
					arg.SetPattern(pattern);
					if (!arg.IsKey('c')) {
						Console.WriteLine("set count of 512 bytes messages, use key -c");
						return;
					}
					uint count = 0;
					try {
						count = UInt32.Parse(arg.GetKeyValue('c'));
					}
					catch (Exception err) {
						Console.WriteLine("wrong number under key -c");
						return;
					}
					//TODO write code
					IPart req = new PartStruct()
                        .Add("cmd", "stress")
						.Add("data", new PartStruct()
						     .Add("dataCount", count)
						     .Add("clients", getSelectedClients())
						    );
                    JSONParser data = new JSONParser(await client.SendMessageAsync(req.ToJSON()));
					Console.WriteLine(data.ToJSON());

				};
			});
		}

		/*
		* создание команд для справки
		*/
		private static void ConfigHelpCommands() {
			helpArray.AddCommand((c) => {
				c.Name = "clients";
				c.Execute = (CommandData[] arguments) => Console.WriteLine("select all clients flom server" +
				                                                           "\nuse:\tclients [keys]" +
				                                                           "\n\t-g groupe_name" +
				                                                           "\n\t-n iot_name");
			});
			helpArray.AddCommand((c) => {
				c.Name = "exit";
				c.Execute = (CommandData[] arguments) => Console.WriteLine("close client end exit");
			});
			helpArray.AddCommand((c) => {
				c.Name = "current";
				c.Execute = (CommandData[] arguments) => Console.WriteLine("print all kown clients");
			});
			helpArray.AddCommand((c) => {
				c.Name = "help";
				c.Execute = (CommandData[] arguments) => Console.WriteLine("print help by command" +
				                                                           "\nuse:\thelp command");
			});
			helpArray.AddCommand((c) => {
				c.Name = "StressTest";
				c.Execute = (CommandData[] arguments) => Console.WriteLine("run stress test with selected count of messages (one message 512 bytes)" +
				                                                           "use as" +
				                                                           "StressTest -c count");
			});
			helpArray.AddCommand((c) => {
				c.Name = "LoadTest";
				c.Execute = (CommandData[] arguments) => Console.WriteLine("run load test with selected random destripution" +
				                                                           "use as:" +
				                                                           "LoadTest -r type_of_random_destripution" +
				                                                           "\tnormal -m double_num -d double_num" +
				                                                           "\tgamma -a double_num -l double_num" +
				                                                           "\terlang -m uint_num, -l double_num" +
				                                                           "\tpareto -x double_num -a double_num");
			});
		}
	}
}
