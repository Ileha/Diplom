using JSONParserLibrary;
using MyWebSocket.RR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTServer.Commands
{
    class CommandLoadTest : IServerCommand
    {
        public CommandLoadTest(CommonData data)
            : base(data)
        {
            
        }

        private IPart TestForSingleIotClient(Client currentClient, string req, uint count)
        {
            try
            {
                RRClient iotClient = currentClient.getConnect();
                JSONParser reply = new JSONParser(iotClient.SendMessageAsync(req).Result); //{ok: {session_id}}

                uint sessionId = reply["ok.session_id"].GetValue<UInt32>();

                return data.statisticData.GetMetric(currentClient.Address.Address, sessionId, count).GetJsonData();
            }
            catch (Exception err) {
                return new PartStruct().Add("exception", err.Message);
            }
        }

        /*
        requ	{"data_count": 1000, "distribution": "normal", "m": 0, "d": 1, "clients":[]}
        */
        public override void Execute(ClientData argument) {
            uint count = argument.data["data_count"].GetValue<uint>();
            IPart hid = argument.data["clients"];

            argument.data.Data.Remove("clients");

            Task<IPart>[] workers = new Task<IPart>[data.clients.Count];

            string req = new PartStruct()
                .Add("cmd", "load")
                .Add("data", argument.data.Data).ToJSON();
            int index = 0;
            foreach (IPart cli in hid) {
                Client currentClient = data.clients[cli.GetValue<string>()];
                workers[index] = Task.Factory.StartNew(() => {
                    return TestForSingleIotClient(currentClient, req, count);
                });
                index++;
            }

            IPart container = new PartArray();
            IPart sendedMessage = new PartStruct().Add("ok", container);
            Task.WaitAll(workers);
            for (int i = 0; i < workers.Length; i++)
            {
                container.Add(workers[i].Result);
            }
            argument.client.SendMessageAsync(sendedMessage.ToJSON());
        }
    }
}
