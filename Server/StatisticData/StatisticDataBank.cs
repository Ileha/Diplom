using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace IOTServer.StatisticData {
	public class StatisticDataBank {
	 	private const int AWAIT_TIMEOUT = 2000;

		private Dictionary<SessionID, List<StatisticElement>> data;

		public StatisticDataBank() {
			data = new Dictionary<SessionID, List<StatisticElement>>();
		}

		public int GetDataCount(IPAddress client, uint sessionID) {
			SessionID id = new SessionID(client, sessionID);
			try {
				return data[id].Count;
			}
			catch (Exception err) {
				return 0;
			}
		}

		public List<StatisticElement> Pop(IPAddress client, uint sessionID) {
			SessionID id = new SessionID(client, sessionID);
			return Pop(id);
		}
		public List<StatisticElement> Pop(SessionID id) {
			List<StatisticElement> res = null;
			lock (data) {
				res = data[id];
				data.Remove(id);
			}
			return res;
		}

		public void AddData(IPAddress client, long bytesCount, uint sessionID) {
			SessionID id = new SessionID(client, sessionID);
			StatisticElement add = new StatisticElement(bytesCount);
			lock(data) {
				List<StatisticElement> statisticSession = null;
				if (!data.ContainsKey(id)) {
					statisticSession = new List<StatisticElement>();
					data.Add(id, statisticSession);
				}
				else {
					statisticSession = data[id];
				}
				statisticSession.Add(add);
			}

		}
	}

	public class StatisticDataAwaiter : IDisposable {
		public TaskCompletionSource<List<StatisticElement>> task { get; private set; }
		private Timer timer;
		private Action onEnd;
		private int count;

		public StatisticDataAwaiter(Action onEnd, int count, int timeout) {
			this.count = count;
			this.onEnd = onEnd;
			task = new TaskCompletionSource<List<StatisticElement>>();
			timer = new Timer(
				OnEvent,
				null, 
				timeout, //dueTime
				Timeout.Infinite //period
			);
		}

		public void Dispose() {
			timer.Dispose();
		}

		public void CheckCondition(int count, uint nextDelay) {
			if (count >= this.count) {
				OnEvent(null);
			}
			else {
				UpdateTimer(nextDelay);
			}
		}

		private void UpdateTimer(uint timeout) {
			timer.Change(timeout, Timeout.Infinite);
		}

		private void OnEvent(object state) {
            Dispose();
			onEnd();
		}
	}
}
