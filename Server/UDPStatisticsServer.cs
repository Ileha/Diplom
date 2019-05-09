using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace IOTServer
{
	public class UDPStatisticsServer : IDisposable
	{
		private Thread listner;
		private UdpClient client;
		public UInt16 targetPort { get; private set; }
		private Action<IPEndPoint, BinaryReader> onMessage;

		public UDPStatisticsServer(UInt16 targetPort, Action<IPEndPoint, BinaryReader> onMessage) {
			this.onMessage = onMessage;
			this.targetPort = targetPort;
			listner = new Thread(() => listen());
			listner.IsBackground = false;
			client = new UdpClient(new IPEndPoint(IPAddress.Parse("0.0.0.0"), targetPort));
			listner.Start();
		}

		private void listen() {
			try {
				while (true) {
					IPEndPoint ip = null;
					byte[] data = client.Receive(ref ip);
					ThreadPool.QueueUserWorkItem((state) => {
						using (BinaryReader reader = new BinaryReader(new MemoryStream(data))) {
							onMessage(ip, reader);
						}
					});
				}
			}
			catch (ThreadAbortException abrot) {}
		}

		public void Dispose()
		{
			listner.Abort();
			client.Close();
		}
	}
}
