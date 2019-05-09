using System;

namespace UiserClient
{
	public class Client
	{
		public string Address { get; private set; }
		public string Name { get; private set; }
		public string Groupe { get; private set; }
		public string HID { get; private set; }

		public Client(string address, string name, string groupe, string hid) {
			Address = address;
			Name = name;
			Groupe = groupe;
			HID = hid;
		}

		public override string ToString()
		{
			return string.Format("Client: Name {1} on address {0}", Address, Name);
		}
	}
}
