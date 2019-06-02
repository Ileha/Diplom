using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace MyWebSocket
{
    internal class ThreadPool : IDisposable
    {
        private ConcurrentQueue<Action> actionQueue;
        private ManualResetEvent hasNext;
        private Thread[] pool;

        internal ThreadPool(int threadCount)
        {
            actionQueue = new ConcurrentQueue<Action>();
            hasNext = new ManualResetEvent(false);
            pool = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                pool[i] = new Thread(execute);
                pool[i].IsBackground = true;
                pool[i].Start();
            }
        }

        private void execute()
        {
            try
            {
                while (true)
                {
                    hasNext.WaitOne();
                    Action currentTask = null;
                    if (actionQueue.TryDequeue(out currentTask))
                    {
                        currentTask();
                    }
                    else
                    {
                        hasNext.Reset();
                    }
                }
            }
            catch (ThreadAbortException abrot) { }
        }

        public void addTask(Action task)
        {
            actionQueue.Enqueue(task);
            hasNext.Set();
        }

        public void Dispose()
        {
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i].Abort();
            }
        }
    }
}
