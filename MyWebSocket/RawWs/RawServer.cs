using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWebSocket;
using System.Net.Sockets;

namespace MyWebSocket.RawWs
{
    public delegate void socketEvent(RawSocket socket);
    public delegate void empty();
    public delegate void error(Exception err);
    public delegate void message(string message, RawSocket socket);

    public class RawServer : Server
    {
        public event socketEvent onOpen;
        public event empty onClose;
        public event error onError;
        public event message onMessage;

        public RawServer(string url)
            : base(url)
        {
            
        }

        internal void OnMessage(string message, RawSocket socket) {
            if (onMessage != null) {
                onMessage(message, socket);
            }
        }

        protected override void OnClose()
        {
            if (onClose != null) { onClose(); }
        }

        protected override void OnError(Exception err)
        {
            if (onError != null) { onError(err); }
        }

        protected override void OnOpen(TcpClient client) {
            if (onOpen != null) {
                onOpen(new RawSocket(client, isEncrypt, this));
            }
        }
    }
}
