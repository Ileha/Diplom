using System;
using System.Net;
using System.Threading.Tasks;

namespace MyWebSocket.RR
{
	/*
	* ничем не отличается от RRClient но при отправке сообщения не создаёт запись в waitRequests,
	* а сразу отправляет сообщение как ответ, дописывая в начало id сообщения.
	* Отправить ответ можно только один раз
	*/
	internal class RRClientWrapper : IRRClient
	{
		private IRRClient original;
		private Guid id;
		private Action<string> sendMessage;
		private bool just_sent = false;

		public IPAddress address {
			get {
				return original.address;
			}
		}

		public IPEndPoint EndPoint {
			get {
				return original.EndPoint;
			}
		}

		public bool isEncrypt {
			get {
				return original.isEncrypt;
			}
		}

		public int port {
			get {
				return original.port;
			}
		}

		internal RRClientWrapper(IRRClient original, Guid id, Action<string> sendMessage) {
            this.original = original;
			this.id = id;
			this.sendMessage = sendMessage;
		}

		public void Close() {
            original.Close();
		}

		public void SendMessage(string message) {
			throw new NotSupportedException("use another async method");
		}

		public Task<string> SendMessageAsync(string message) {
            if (just_sent) { throw new Exception("just used"); /* TODO exception */ }
			just_sent = true;
			TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
			tcs.SetResult("");
			sendMessage(id.ToString() + message);
			return tcs.Task;
		}

		public void AddOnCloseObserver(Action<IRRClient> observer)
		{
            original.AddOnCloseObserver(observer);
		}

		public bool IsClosed()
		{
			return original.IsClosed();
		}
	}
}
