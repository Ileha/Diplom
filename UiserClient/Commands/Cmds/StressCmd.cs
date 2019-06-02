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
            Console.WriteLine(replyData.ToJSON());
        }
    }
}
