using JSONParserLibrary;
using MyWebSocket.RR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTServer.Commands
{
    class CommandPerformance : IServerCommand
    {
        public CommandPerformance(CommonData dataHolder) : base(dataHolder) {}
        private IPart TestForSingleIotClient(Client currentClient, string req) {
            try
            {
                RRClient iotClient = currentClient.getConnect();
                JSONParser reply = new JSONParser(iotClient.SendMessageAsync(req).Result); //{ok: {generate, sort, search}}
                return reply["ok"];
            }
            catch (Exception err) {
                return new PartStruct().Add("exception", err.Message);
            }
        }

        public override void Execute(ClientData argument) {
            IPart hid = argument.data["clients"];

            Task<IPart>[] workers = new Task<IPart>[data.clients.Count];

            string req = new PartStruct()
                .Add("cmd", "performance")
                .Add("data", new PartStruct()).ToJSON();

            int index = 0;
            foreach (IPart cli in hid) {
                Client currentClient = data.clients[cli.GetValue<string>()];
                workers[index] = Task.Factory.StartNew(() =>
                {
                    return TestForSingleIotClient(currentClient, req);
                });
                index++;
            }
            IPart container = new PartArray();
            IPart sendedMessage = new PartStruct().Add("ok", container);
            Task.WaitAll(workers);
            for (int i = 0; i < workers.Length; i++) {
                container.Add(workers[i].Result);
            }
            argument.client.SendMessageAsync(sendedMessage.ToJSON());
        }
    }
}
