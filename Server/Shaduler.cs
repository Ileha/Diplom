using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace IOTServer
{
	public class ShadulerTask {
		public uint id { get; private set; }
		public int delay;//mc
		public long unixTimestamp;
		public bool loop = false;
		public Action action;

		public ShadulerTask(uint id) {
			this.id = id;
		}

		public override string ToString() {
			return string.Format("[ShadulerTask unixTimestamp = {0}, id = {1}]", unixTimestamp, id);
		}

		public override bool Equals(object obj) {
			if (obj == null) { return false; }
			if (!(obj is ShadulerTask)) { return false; }

			ShadulerTask other = obj as ShadulerTask;
			if (other == this) { return true; }

			return id == other.id;
		}
	}

	public class Shaduler {
		private LinkedList<ShadulerTask> shadulerTasks = new LinkedList<ShadulerTask>();
		private DateTime unixStart = new DateTime(1970, 1, 1);

		private object locker = new object();
		private uint forId = 0;

		private Timer timer;

		private ShadulerTask current = null;

		public Shaduler() {
			timer = new Timer(
				(state) => {
					ShadulerTask forExecute = null;
					LockOperations((obj) => {
						forExecute = obj.First.Value;
						obj.RemoveFirst();
					});
					
					ThreadPool.QueueUserWorkItem((stateTP) => { forExecute.action(); });
					if (forExecute.loop) {
						AddShadulerTask(forExecute);
					}
					Reconfig();
				},
				null, 
				Timeout.Infinite, //dueTime
				Timeout.Infinite //period
			);
		}

		public uint AddShadulerTask(Action<ShadulerTask> configurate) {
			ShadulerTask forAdd = new ShadulerTask(GetNextID());
			configurate(forAdd);

			AddShadulerTask(forAdd);

			if (IsFirst(forAdd)) {
				Reconfig();
			}

			return forAdd.id;
		}

		private uint AddShadulerTask(ShadulerTask forAdd) {
			forAdd.unixTimestamp = GetUnixTimestamp() + forAdd.delay;

			//25    35
			//   30    40
			LockOperations((LinkedList<ShadulerTask> obj) => {
				LinkedListNode<ShadulerTask> node = obj.First;
				bool isFirst = node == null;

				while (node != null && node.Value.unixTimestamp < forAdd.unixTimestamp) {
					node = node.Next;
				}

				if (node == null) {
					if (isFirst) {
						obj.AddFirst(forAdd);
					}
					else {
						obj.AddLast(forAdd);
					}
				}
				else {
					obj.AddBefore(node, forAdd);
				}
			});

			return forAdd.id;
		}

		private bool IsFirst(ShadulerTask task) {
			return shadulerTasks.First.Value.Equals(task);
		}

		private void Reconfig() {
			timer.Change(Timeout.Infinite, Timeout.Infinite);

			ShadulerTask forExecute = null;
			LockOperations((obj) => {
				forExecute = shadulerTasks.First.Value;
			});

			if (forExecute != null) {
				//get next
				long delay = forExecute.unixTimestamp - GetUnixTimestamp();
				timer.Change(delay, Timeout.Infinite);
			}
			else {
				//stop
			}
		}

		private void LockOperations(Action<LinkedList<ShadulerTask>> operations) {
			lock (shadulerTasks) {
				operations(shadulerTasks);
			}
		}

		private long GetUnixTimestamp() {
			return (long)(DateTime.UtcNow.Subtract(unixStart)).TotalMilliseconds;
		}

		private uint GetNextID() {
			uint id = 0;
			lock(locker) {
				id = forId++;
			}
			return id;
		}
	}
}