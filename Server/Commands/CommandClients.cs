using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTServer.Commands
{
    public class CommandClients : IServerCommand
    {
        public CommandClients(CommonData data) : base(data) {

        }

        //{name, groupe}
        public override void Execute(ClientData argument) {
            PartArray res = new PartArray();

            IPart ng = null;

            Func<Client, bool> nameReact = (arg) => true;
            Func<Client, bool> groupeReact = (arg) => true;

            if (argument.data.Data.ByPathSave("name", out ng)) {
                string targetName = ng.GetValue<string>();
                nameReact = (arg) => arg.Name.Equals(targetName);
            }
            if (argument.data.Data.ByPathSave("groupe", out ng)) {
                string targetGroupe = ng.GetValue<string>();
                nameReact = (arg) => arg.Groupe.Equals(targetGroupe);
            }

            IEnumerable<KeyValuePair<String, Client>> query = (from cli in data.clients
                                                               where nameReact(cli.Value) == true &&
                                                               groupeReact(cli.Value) == true
                                                               select cli);
            foreach (KeyValuePair<String, Client> client in query)
            {
                res.Add(new PartStruct()
                        .Add("address", String.Format("{0}:{1}", client.Value.Address.Address, client.Value.Address.Port))
                        .Add("name", client.Value.Name)
                        .Add("groupe", client.Value.Groupe)
                        .Add("id", client.Key));
            }
            argument.client.SendMessageAsync(res.ToJSON());
        }
    }
}
