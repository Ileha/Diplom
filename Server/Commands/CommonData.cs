using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOTServer;
using IOTServer.StatisticData;

namespace IOTServer.Commands
{
    /*
     * хранит в себе данные о клиентах и статистику
     */
    public class CommonData
    {
        public StatisticDataBank statisticData { get; private set; }
        public Dictionary<String, Client> clients { get; private set; }

        public CommonData() {
            statisticData = new StatisticDataBank();
            clients = new Dictionary<string, Client>();
        }
    }
}
