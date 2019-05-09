using System;
using MyWebSocket.RR;
using System.Net;

namespace IOTServer {
	public class Client {
		public IPEndPoint Address { get; private set; }
		public string Name { get; private set; }
		public string Groupe { get; private set; }

		public Client(IPEndPoint address, string name, string groupe) {
			Address = address;
			Name = name;
			Groupe = groupe;
		}

		public void Update(IPEndPoint address, string name, string groupe) {
			Address = address;
			Name = name;
			Groupe = groupe;
		}

		public RRClient getConnect() {
			return new RRClient(String.Format("ws://{0}:{1}", Address.Address, Address.Port));
		}

        public override string ToString()
        {
            return String.Format("{0} in groupe {1}", Name, Groupe);
        }
		//public override int GetHashCode() {
		//	return 17+ Name.GetHashCode() * 17 + Groupe.GetHashCode();
		//}

		//public override bool Equals(object obj) {
		//	if (obj == null) { return false; }
		//	if (!(obj is Client)) { return false; }

		//	Client other = obj as Client;
		//	if (other == this) { return true; }

		//	return Name.Equals(other.Name) 
		//		          && Groupe.Equals(other.Groupe);
		//}
	}
}
