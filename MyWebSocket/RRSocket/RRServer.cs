using System;
using System.Net.Sockets;

namespace MyWebSocket.RR
{
	/*
	* создаёт RRClient по TcpClient
	*/
	public class RRServer : Server
	{
		private Action<string, IRRClient> messageHandler;
		private Action<RRClient> onConnect;
		
		public RRServer(String url, Action<string, IRRClient> onMessage, Action<IRRClient> onConnect) : base(url) {
			messageHandler = onMessage;
			this.onConnect = onConnect;
		}

        protected override void OnOpen(TcpClient client)
		{
			RRClient rRClient = new RRClient(client, isEncrypt, messageHandler);
			onConnect(rRClient);
		}

		protected override void OnClose() {

		}

		protected override void OnError(Exception err)
		{
			Console.WriteLine("RRServer err: {0}", err);
		}
	}
}
