using System;
using System.Net;
using System.Threading.Tasks;

namespace MyWebSocket.RR
{
	public interface IRRClient
	{
		int port { get; }
		IPAddress address { get; }
		bool isEncrypt { get; }
		IPEndPoint EndPoint { get; }

		bool IsClosed();
		void SendMessage(String message);
		void Close();

		Task<string> SendMessageAsync(string message);
		void AddOnCloseObserver(Action<IRRClient> observer);
	}
}
