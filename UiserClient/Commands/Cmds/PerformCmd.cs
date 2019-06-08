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
        }
    }
}
