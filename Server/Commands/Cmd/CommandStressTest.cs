using IOTServer.StatisticData;
using JSONParserLibrary;
using MyWebSocket.RR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTServer.Commands
{
    public class CommandStressTest : IServerCommand {
        public CommandStressTest(CommonData data) : base(data) {
        
        }

        //{dataCount, clients:[]}
        private IPart TestForSingleIotClient(Client currentClient, string req, uint count)
        {
            try {
                RRClient iotClient = currentClient.getConnect();
                JSONParser reply = new JSONParser(iotClient.SendMessageAsync(req).Result); //{ok: {session_id}}

                uint sessionId = reply["ok.session_id"].GetValue<UInt32>();

                return data.statisticData.GetMetric(currentClient.Address.Address, sessionId, count).GetJsonData();
            }
            catch (Exception err) {
                return new PartStruct().Add("exception", err.Message);
            }
        }

        public override void Execute(ClientData argument) {
            uint count = argument.data["dataCount"].GetValue<uint>();
            IPart hid = argument.data["clients"];

            Task<IPart>[] workers = new Task<IPart>[hid.Count];
            Client[] selected_clients = new Client[hid.Count];

            string req = new PartStruct()
                .Add("cmd", "stress")
                .Add("data", new PartStruct()
                     .Add("data_count", count)).ToJSON();

            int index = 0;
            foreach (IPart cli in hid) {
                Client currentClient = data.clients[cli.GetValue<string>()];
                selected_clients[index] = currentClient;
                workers[index] = Task.Factory.StartNew(() => {
                    return TestForSingleIotClient(currentClient, req, count);
                });
                index++;
            }

            IPart container = new PartArray();
            IPart sendedMessage = new PartStruct().Add("ok", container);
            Task.WaitAll(workers);
            for (int i = 0; i < workers.Length; i++) {
                container.Add(new PartStruct().Add("name", selected_clients[i].ToString()).Add("result", workers[i].Result));
            }
            argument.client.SendMessageAsync(sendedMessage.ToJSON());
        }
    }
}
