using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IOTClient
{
	public class FlagReceiver : IDisposable
	{
		private Thread listner;
		private UdpClient client;
		private Action<IPEndPoint, byte[]> onReceiveData;
		public UInt16 targetPort { get; private set; }

		public FlagReceiver(UInt16 targetPort, Action<IPEndPoint, byte[]> onReceiveData) {
            this.onReceiveData = onReceiveData;
			this.targetPort = targetPort;
			listner = new Thread(() => listen());
			listner.IsBackground = false;
			client = new UdpClient(new IPEndPoint(IPAddress.Parse("0.0.0.0"), targetPort));
			client.EnableBroadcast = true;
			listner.Start();
		}

		public void Dispose() {
			listner.Abort();
			client.Close();
		}

		private void listen()
		{
			try
			{
				while (true)
				{
					IPEndPoint ip = null;
					byte[] data = client.Receive(ref ip);
					ThreadPool.QueueUserWorkItem((state) => onReceiveData(ip, data));
				}
			}
			catch (ThreadAbortException abrot) {}
		}

	}
}
