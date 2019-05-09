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
            RRClient iotClient = currentClient.getConnect();
            JSONParser reply = new JSONParser(iotClient.SendMessageAsync(req).Result); //{ok: {session_id}}
            
            uint sessionId = reply["ok.session_id"].GetValue<UInt32>();
            
            List<StatisticElement> statistic = data.statisticData.Pop(iotClient.address, sessionId);
            
            double all = statistic.Count;
            double all_time = statistic[(int)all - 1].UnixTime - statistic[0].UnixTime;

            double speed = double.PositiveInfinity;
            if (all_time > 0) {
                speed = ((all * 512.0d * 8.0d) / all_time) / 1000.0d; //в Мбит/c
            }
            double jitter = 0;
            double delay = double.PositiveInfinity;
            if (all > 0) {
                delay = (all_time / all) / 1000; //в секундах
                for (int i = 1; i < all; i++) {
                    jitter += Math.Pow((delay - (statistic[i].UnixTime - statistic[i - 1].UnixTime)/1000), 2);
                }
                jitter /= all;
            }

            return new PartStruct()
                .Add("name", currentClient.ToString())
                .Add("result", new PartStruct()
                        .Add("jitter", String.Format("{0} c", jitter))
                        .Add("delay", String.Format("{0} c", delay))
                        .Add("speed", String.Format("{0} Mbit/c", speed))
                        .Add("missed", count - all)
                    );
        }

        public override void Execute(ClientData argument) {
            uint count = argument.data["dataCount"].GetValue<uint>();
            IPart hid = argument.data["clients"];

            Task<IPart>[] workers = new Task<IPart>[data.clients.Count];

            string req = new PartStruct()
                .Add("cmd", "stress")
                .Add("data", new PartStruct()
                     .Add("data_count", count)).ToJSON();

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
            for (int i = 0; i < workers.Length; i++) {
                container.Add(workers[i].Result);
            }
            argument.client.SendMessageAsync(sendedMessage.ToJSON());
        }
    }
}
