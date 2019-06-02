using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWebSocket;
using System.Net.Sockets;

namespace MyWebSocket.RawWs
{
    public class RawSocket : WebSocket
    {
        private RawServer server;

        public event socketEvent onClose;
        public event error onError;

        private event message internalOnMessage;
        public event message onMessage {
            add {
                if (server == null)
                {
                    internalOnMessage += value;
                }
                else {
                    throw new NotSupportedException("use server for this");
                }
            }
            remove {
                if (server == null)
                {
                    internalOnMessage -= value;
                }
                else
                {
                    throw new NotSupportedException("use server for this");
                }
            }
        }

        public RawSocket(string url) : base(url) {
            Start();
		}
        internal RawSocket(TcpClient client, bool encryption, RawServer server)
            : base(client, encryption)
        {
            this.server = server;
            Start();
		}

        protected override void OnMessage(string message)
        {
            if (server == null) {
                if (internalOnMessage != null) {
                    internalOnMessage(message, this);
                }
            }
            else {
                server.OnMessage(message, this);
            }
        }

        protected override void OnError(Exception err)
        {
            if (onError != null) { onError(err); }
        }

        protected override void OnOpen() {}

        protected override void OnClose() {
            if (onClose != null) { onClose(this); }
        }
    }
}
