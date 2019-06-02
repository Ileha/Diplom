using System;
using CommadInterfaces;
using MyWebSocket.RR;
using JSONParserLibrary;
using System.Collections.Generic;
using System.Threading;
using UiserClient.Commands;
using UiserClient.Commands.Cmds;

namespace UiserClient
{
	class MainClass
	{
        private static CommonData data;
        private static CommandArray<CommandData> cmdArray = new CommandArray<CommandData>();
		private static Thread mainThread;

		public static void Main(string[] args) //ip, port
		{
            data = new CommonData(String.Format("ws://{0}:{1}", args[0], args[1]));
            //data = new CommonData(String.Format("ws://{0}:{1}", "127.0.0.1", "20146"));
			mainThread = Thread.CurrentThread;

			ConfigMainCommands();

			data.server.onClose += (IRRClient mainclient) => {//при падении сервера програма завершает работу
				if (data.execute) {
					Console.WriteLine("server is droped");
					cmdArray.Execute("exit", null);
					mainThread.Abort();
				}
			};

			//cmd [ARGS] [OPTIONS]
			try
			{
                while (data.execute)
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
						cmdArray.Execute(splited.Cmd, splited);
					}
					catch (Exception err) {
						Console.WriteLine(err);
					}
				}
			}
			catch (ThreadAbortException err) {
				
			}
		}

		private static void ConfigMainCommands() {
			cmdArray.AddCommand(c => {//выводит список выбранных клиентов
			    c.Name = "current";
			    c.Execute = (CommandData[] arguments) => {
					foreach (Client cli in data.selected) {
						Console.WriteLine(cli);
					}
				};
			});
			cmdArray.AddCommand(c => {//запрашивает у сервера клинтов по имени и или группе и выбирает их
                CommandDataPattern pattern = new CommandDataPattern()
                    .AddOption('n', true)
                    .AddOption('g', true);
                ICommand cmd = new ClientsCmd(data, pattern);
                c.Name = "clients";
			    c.Execute = (CommandData[] arguments) => {
                    cmd.Execute(arguments[0]);
				};
			});
			cmdArray.AddCommand(c => {//завершает работу програмы
				c.Name = "exit";
				c.Execute = (CommandData[] arguments) => {
					if (!data.server.IsClosed()) {
                        data.server.Close();
					}
					data.execute = false; 
				};
			});
			cmdArray.AddCommand(c => {//записывает в консоль руководство по команде
                ICommand cmd = new HelpCmd(data, CommandDataPattern.EmptyPattern);
                c.Name = "help";
				c.Execute = (CommandData[] arguments) => {
                    cmd.Execute(arguments[0]);
				};
			});

			cmdArray.AddCommand(c => {
				CommandDataPattern pattern = new CommandDataPattern()
					.AddOption('c', true);
                ICommand cmd = new StressCmd(data, pattern);
				c.Name = "stress";
				c.Execute = (CommandData[] arguments) => {
                    cmd.Execute(arguments[0]);
				};
			});

            cmdArray.AddCommand(c => {
                CommandDataPattern pattern = new CommandDataPattern()
                    .AddOption('r', true)
                    .AddOption('m', true)
                    .AddOption('d', true)
                    .AddOption('a', true)
                    .AddOption('l', true)
                    .AddOption('x', true)
                    .AddOption('c', true);
                ICommand cmd = new LoadCmd(data, pattern);
                c.Name = "load";
                c.Execute = (CommandData[] arguments) =>
                {
                    cmd.Execute(arguments[0]);
                };
            });
            
            cmdArray.AddCommand(c => {
                ICommand cmd = new PerformCmd(data, CommandDataPattern.EmptyPattern);
                c.Name = "perform";
                c.Execute = (CommandData[] arguments) =>
                {
                    cmd.Execute(arguments[0]);
                };
            });
		}
	}
}
