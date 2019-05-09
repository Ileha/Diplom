using System;
using JSONParserLibrary;
using MyWebSocket.RR;

namespace IOTClient {
	public class ClientData
	{
		public IRRClient client { get; private set; }
		public JSONParser data { get; private set; }

		public ClientData(IRRClient client, JSONParser data) {
			this.client = client;
			this.data = data;
		}
	}
}
