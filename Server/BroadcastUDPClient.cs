using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace IOTServer
{
	public class BroadcastUDPClient : IDisposable
	{
		private Timer timer;
		private UdpClient broadcastClient;
		private byte[] data;
		public IPEndPoint target { get; private set; }

		public BroadcastUDPClient(int millisecondsCount, ushort targetPort, string ipAddress, string mask, byte[] message) {
			//broadcastClient = new UdpClient(new IPEndPoint(GetBroadcastAddress(IPAddress.Parse(ipAddress), IPAddress.Parse(mask)), targetPort));
			broadcastClient = new UdpClient();
			broadcastClient.EnableBroadcast = true;
			target = new IPEndPoint(GetBroadcastAddress(IPAddress.Parse(ipAddress), IPAddress.Parse(mask)), targetPort);
			TimerCallback tm = new TimerCallback(Action);
			timer = new Timer(tm, null, 0, millisecondsCount);
			data = message;
		}

		public void Action(object obj) {
			//Console.WriteLine("send!!!");
			broadcastClient.Send(data, data.Length, target);
		}

		public void Dispose() {
			timer.Dispose();
			broadcastClient.Close();
		}

		private static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask) {
			byte[] ipAdressBytes = address.GetAddressBytes();
			byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

			if (ipAdressBytes.Length != subnetMaskBytes.Length)
				throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

			byte[] broadcastAddress = new byte[ipAdressBytes.Length];
			for (int i = 0; i < broadcastAddress.Length; i++)
			{
				broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
			}
			return new IPAddress(broadcastAddress);
		}
	}
}
