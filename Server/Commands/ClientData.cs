using System;
using JSONParserLibrary;
using MyWebSocket.RR;

namespace IOTServer.Commands {

    /*
     * данные представляющие собой запрос от клиента и самого клиента которому нужно ответить
     */
	public class ClientData
	{
		public IRRClient client { get; private set; }
		public JSONParser data { get; private set; }

		public ClientData(IRRClient client, JSONParser data) {
			this.client = client;
			this.data = data;
		}
	}
	public class ExtendedClientData {
		public JSONParser IOTClientData { get; private set; }
		public Client IOTClient { get; private set; }
		public IRRClient client { 
			get {
				return pattern.client;	
			}
		}
		public JSONParser data { 
			get {
				return pattern.data;
			}
		}

		private ClientData pattern;

		public ExtendedClientData(ClientData pattern, JSONParser IOTClientData, Client IOTClient)
		{
			this.pattern = pattern;
            this.IOTClientData = IOTClientData;
			this.IOTClient = IOTClient;
		}
	}
}
