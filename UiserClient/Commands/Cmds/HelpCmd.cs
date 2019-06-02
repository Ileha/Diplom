using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiserClient.Commands.Cmds
{ 
    class HelpCmd : ICommand
    {
        private Dictionary<String, String> helpData;
        public HelpCmd(CommonData data, CommandDataPattern pattern) 
            : base(data, pattern) 
        {
            helpData = new Dictionary<string, string>();
            helpData.Add("clients", "select all clients flom server\n" +
                                    "use:\tclients [keys]\n" +
                                    "\t-g groupe_name\n" +
                                    "\t-n iot_name");
            helpData.Add("exit", "close client end exit");
            helpData.Add("current", "print all known clients");
            helpData.Add("help", "print help by command\n" +
                                 "use:\thelp command");
            helpData.Add("stress", "run stress test with selected count of messages (one message 512 bytes)\n" +
                                   "use as\n" +
                                   "\tstress -c count");
            helpData.Add("load", "run load test with selected random distribution\n" +
                                 "use as:\n" +
                                 "\tload -r type_of_random_distribution -c count\n" +
                                 "\twhere -r:\n" +
                                 "\t\tnormal -m double_num -d double_num\n" +
                                 "\t\tgamma -a double_num -l double_num\n" +
                                 "\t\terlang -m uint_num, -l double_num\n" +
                                 "\t\tpareto -x double_num -a double_num\n");
        }

        protected override void execute(CommandData argument) {
            if (argument.args.Count > 0) {
                Console.WriteLine(helpData[argument.args[0]]);
            }
            else {
                Console.WriteLine("all avaliable commands\n"+
                    "for more information use help [command name]");
                foreach (string name in helpData.Keys) {
                    Console.WriteLine("\t{0}", name);
                }
            }
        }
    }
}
