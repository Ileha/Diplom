using System;
using System.Net.Sockets;

namespace MyWebSocket.RR
{
	/*
	* создаёт RRClient по TcpClient
	*/

    public delegate void IRREvent(IRRClient socket);
    public delegate void empty();
    public delegate void error(Exception err);
    public delegate void message(string message, IRRClient socket);

	public class RRServer : Server
	{
        public event IRREvent onOpen;
        public event empty onClose;
        public event error onServerError;
        public event message onMessage;
		
		public RRServer(String url) 
            : base(url) 
        {

		}

        internal void OnMessage(string message, IRRClient socket) {
            if (onMessage != null) {
                onMessage(message, socket);
            }
        }

        protected override void OnOpen(TcpClient client)
		{
			RRClient rRClient = new RRClient(client, isEncrypt, this);
            if (onOpen != null) {
                onOpen(rRClient);
            }
		}

		protected override void OnClose() {
            if (onClose != null) { onClose(); }
		}

		protected override void OnError(Exception err)
		{
            if (onServerError != null) { onServerError(err); }
		}
	}
}
