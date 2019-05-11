using System;
using System.Net.Sockets;
using System.Threading;

namespace MyWebSocket
{

	/*
	* пердставляет собой слушателя и вызывает OnOpen с TcpClient
	*/
	public abstract class Server : Connect, IDisposable
	{
		private Thread task;
		private TcpListener listener;

		public Server(String url) : base(url)
		{
			task = new Thread(OnConnectHandler);
			task.IsBackground = false;
			listener = new TcpListener(address, port);
			task.Start();
		}

		public void Close() {//add needs code for stop task
			task.Abort();
		}

		protected abstract void OnClose();
		protected abstract void OnError(Exception err);
		protected abstract void OnOpen(TcpClient client);

		private void OnConnectHandler() {
			try
			{
				listener.Start();

				while (true)
				{
					TcpClient client = listener.AcceptTcpClient();
					OnOpen(client);
				}
			}
			catch (ThreadAbortException abrot) {}
			catch (Exception err)
			{
				OnError(err);
			}
			finally
			{
				if (listener != null) {
					listener.Stop();
				}
				OnClose();
			}
		}

		public void Dispose() {
			Close();
		}
	}
}
