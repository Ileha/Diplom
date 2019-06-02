using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MyWebSocket.RR
{
	/*
	* представляет собой класс для работы с запросами и ответами
	*/

	public class RRClient : WebSocket, IRRClient
	{
        public event IRREvent onClose;
        private RRServer server;
        public event error onError;
        private ConcurrentDictionary<Guid, TaskCompletionSource<string>> waitRequests;

		public RRClient(string url) 
            : base(url) 
        {
            waitRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<string>>();
            Start();
        }
		internal RRClient(TcpClient client, bool encryption, RRServer server) 
            : base(client, encryption)
		{
			this.server = server;
            Start();
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
            if (waitRequests.TryAdd(id, tcs)) {
                base.SendMessage(id.ToString() + message);
                return tcs.Task;
            }
            else {
                throw new Exception("can't send");
            }
		}

		protected override void OnClose() {
			if (onClose != null) {
                onClose(this);
			}
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
            
            if (server != null) {
                server.OnMessage(message.Substring(36), new RRClientWrapper(this, id, base.SendMessage));
            }
            else {
                TaskCompletionSource<string> awaiter;
                if (waitRequests.TryRemove(id, out awaiter)) {
                    awaiter.SetResult(message.Substring(36));
                }
                else {
                    Console.WriteLine("missed guid");
                }
            }
		}

		protected override void OnOpen() {}
    }
}
