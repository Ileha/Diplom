using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiserClient.Commands.Cmds
{
    class ClientsCmd : ICommand
    {
        public ClientsCmd(CommonData data, CommandDataPattern pattern) 
            : base(data, pattern) 
        {}

        //{cmd:clients, data:{name, groupe}}
        protected override void execute(CommandData argument)
        {
            IPart reqData = new PartStruct();
            IPart req = new PartStruct()
                .Add("cmd", "clients")
                .Add("data", reqData);
            if (argument.IsKey('n')) {
                reqData.Add("name", argument.GetKeyValue<String>('n'));
            }
            if (argument.IsKey('g')) {
                reqData.Add("groupe", argument.GetKeyValue<String>('g'));
            }

            JSONParser replyData = new JSONParser(data.server.SendMessageAsync(req.ToJSON()).Result);
            data.selected.Clear();

            Console.WriteLine("{0} client(s) have found", replyData.Data.Count);
            foreach (IPart part in replyData.Data)
            {
                data.selected.Add(new Client(part.ByPath("address").GetValue<string>(),
                                             part.ByPath("name").GetValue<string>(),
                                             part.ByPath("groupe").GetValue<string>(),
                                             part.ByPath("id").GetValue<string>()
                                             ));
            }
        }
    }
}
