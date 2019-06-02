using JSONParserLibrary;
using MyWebSocket.RR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiserClient.Commands
{
    public class CommonData
    {
        public bool execute = true;
        public RRClient server;
        public List<Client> selected = new List<Client>();

        public CommonData(String url) {
            server = new RRClient(url);
        }

        public PartArray GetSelectedClients() {
            if (selected.Count == 0) {
                throw new Exception("nothing to test");
            }
            PartArray res = new PartArray();
            foreach (Client iotClient in selected) {
                res.Add(iotClient.HID);
            }
            return res;
        }
    }
}
