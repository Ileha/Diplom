using System;
using System.Net;

namespace IOTServer.StatisticData
{
	public class SessionID
	{
		public IPAddress client { get; private set; }
		public uint iD { get; private set; }

		public SessionID(IPAddress client, uint sessionID) {
			this.client = client;
			iD = sessionID;
		}

		public override int GetHashCode() {
			//int hash = client.GetHashCode() * 17 + iD.GetHashCode();
			//Console.WriteLine(hash);
			return client.GetHashCode() * 17 + iD.GetHashCode();
		}

		public override bool Equals(object obj) {
			if (obj == null) { return false; }
			if (!(obj is SessionID)) { return false; }

			SessionID other = obj as SessionID;
			if (other == this) { return true; }

			return client.Equals(other.client) 
				          && iD == other.iD;
		}
	}
}
