using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiserClient.Commands.Cmds
{
    class StressCmd : ICommand
    {
        public StressCmd(CommonData data, CommandDataPattern pattern) 
            : base(data, pattern) 
        {}

        //{cmd: StressTest data: {dataCount, clients:[]}}
        protected override void execute(CommandData argument)
        {
            uint count = argument.GetKeyValue<UInt32>('c');
            
            IPart req = new PartStruct()
                .Add("cmd", "stress")
                .Add("data", new PartStruct()
                     .Add("dataCount", count)
                     .Add("clients", data.GetSelectedClients())
                    );
            JSONParser replyData = new JSONParser(data.server.SendMessageAsync(req.ToJSON()).Result);
            IPart ok = replyData["ok"];
            IPart error = null;
            foreach (IPart enemy in ok) {
                Console.WriteLine(enemy.ByPath("name").GetValue<string>());
                if (enemy.ByPathSave("result.exception", out error)) {
                    Console.WriteLine("\t{0}", error.GetValue<string>());
                }
                else {
                    Console.WriteLine("\tjitter:\t{0}", enemy.ByPath("result.jitter").GetValue<string>());
                    Console.WriteLine("\tdelay:\t{0}", enemy.ByPath("result.delay").GetValue<string>());
                    Console.WriteLine("\tspeed:\t{0}", enemy.ByPath("result.speed").GetValue<string>());
                    Console.WriteLine("\tmissed:\t{0}", enemy.ByPath("result.missed").GetValue<string>());
                }
            }
            //Console.WriteLine(replyData.ToJSON());
            //{"ok":[{"name":"testIOTClient2 in groupe test Group", "result":{"jitter":"2,45700712904944E-09 c", "delay":"4,95685781053388E-05 c", "speed":"82,6329936536718 Mbit/c", "missed":"33,244%"}}]}
        }
    }
}
