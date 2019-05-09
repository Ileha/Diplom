using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyWebSocket.RR
{
	/*
	* представляет собой класс для работы с запросами и ответами
	*/
	public class RRClient : WebSocket, IRRClient
	{
		private Action<IRRClient> observer;
		private Action<string, IRRClient> messageHandler;
		private Dictionary<Guid, TaskCompletionSource<string>> waitRequests = new Dictionary<Guid, TaskCompletionSource<string>>();

		public RRClient(string url) : base(url) {
			//messageHandler = null;
		}
		internal RRClient(TcpClient client, bool encryption, Action<string, IRRClient> onMessage) : base(client, encryption)
		{
			messageHandler = onMessage;
		}

		public void AddOnCloseObserver(Action<IRRClient> observer) {
			this.observer = observer;
		}

		public override void SendMessage(string message)
		{
			throw new NotSupportedException("use another async method");
		}

		/*
		* при отправки сообшения создаёт запись в waitRequests и отправляет
		* сообщение функцией MyWebSocketов
		*/
		public Task<string> SendMessageAsync(string message)
		{
			TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
			Guid id = Guid.NewGuid();
			waitRequests.Add(id, tcs);
			base.SendMessage(id.ToString() + message);
            return tcs.Task;
		}

		protected override void OnClose() {
			if (observer != null) {
				observer(this);
			}
			Console.WriteLine("Closed!!!");
		}

		protected override void OnError(Exception err)
		{
			Console.WriteLine("RRClient err: {0}", err);
		}

		/*
		 * при приёме сообщения решает ответ это или запрос
		 * в первом случае id хранится в waitRequests и нужно закончить Task<string>
         * во втором случае id отсутствует в waitRequests и нужно отправить сообщение на обработку, 
         * а после неё в ответ записать принятый id и отправить
		*/
		protected override void OnMessage(string message)
		{
			Guid id = Guid.Parse(message.Substring(0, 36));
			if (waitRequests.ContainsKey(id)) {
				waitRequests[id].SetResult(message.Substring(36));
                waitRequests.Remove(id);
			}
			else {
				if (messageHandler != null) {
					messageHandler(message.Substring(36), new RRClientWrapper(this, id, base.SendMessage));
				}
			}
		}

		protected override void OnOpen() {}
	}
}
