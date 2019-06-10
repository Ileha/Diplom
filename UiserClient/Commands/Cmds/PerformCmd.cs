using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiserClient.Commands.Cmds
{
    class PerformCmd : ICommand
    {
        public PerformCmd(CommonData data, CommandDataPattern pattern) 
            : base(data, pattern)
        {}

        protected override void execute(CommandData argument) {
            IPart req = new PartStruct()
                .Add("cmd", "performance")
                .Add("data", new PartStruct()
                    .Add("clients", data.GetSelectedClients()));
            JSONParser replyData = new JSONParser(data.server.SendMessageAsync(req.ToJSON()).Result);
            Console.WriteLine(replyData.ToJSON());
            IPart ok = replyData["ok"];
            IPart error = null;
            foreach (IPart enemy in ok) {
                Console.WriteLine(enemy.ByPath("name").GetValue<string>());
                if (enemy.ByPathSave("result.exception", out error)) {
                    Console.WriteLine("\t{0}", error.GetValue<string>());
                }
                else {
                    Console.WriteLine("\tsearch:\t{0}", enemy.ByPath("result.search").GetValue<double>());
                    Console.WriteLine("\tsort:\t{0}", enemy.ByPath("result.sort").GetValue<double>());
                    Console.WriteLine("\tbinary search:\t{0}", enemy.ByPath("result.binary search").GetValue<double>());
                }
            }
            
            //{"ok":[{"name":"testIOTClient2 in groupe test Group", "result":{"search":0.0758679085520745, "sort":0.42851164921216, "binary search":0.149432267030568}}]}
        }
    }
}
