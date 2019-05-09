using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IOTServer.Commands
{
    public class CommandAddClient : IServerCommand {
        public CommandAddClient(CommonData data) : base(data) {
            
        }

        //requ	{data: {name, groupe, id, port}}
        public override void Execute(ClientData argument) {
            string id = argument.data["id"].GetValue<string>();

            lock (data.clients)
            {
                if (data.clients.ContainsKey(id)) {
                    data.clients[id].Update(new IPEndPoint(argument.client.address, argument.data["port"].GetValue<int>()),
                                       argument.data["name"].GetValue<string>(),
                                       argument.data["groupe"].GetValue<string>());
                }
                else
                {
                    data.clients.Add(argument.data["id"].GetValue<string>(),
                                new Client(new IPEndPoint(argument.client.address, argument.data["port"].GetValue<int>()),
                                           argument.data["name"].GetValue<string>(),
                                           argument.data["groupe"].GetValue<string>()));
                }
            }
            argument.client.SendMessageAsync("{\"ok\":{\"message\":\"add\"}}");
        }
    }
}
