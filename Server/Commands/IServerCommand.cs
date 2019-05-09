using IOTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTServer.Commands
{
    public abstract class IServerCommand
    {
        protected CommonData data { get; private set; }

        public IServerCommand(CommonData data) {
            this.data = data;
        }

        public abstract void Execute(ClientData argument);
        protected void SetData(CommonData data) {
            this.data = data;
        }
    }
}
