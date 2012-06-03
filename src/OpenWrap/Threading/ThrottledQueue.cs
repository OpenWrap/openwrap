using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenWrap.Threading
{
    public class ThrottledQueue
    {
        readonly AutoResetEvent _finished = new AutoResetEvent(false);
        readonly Queue<Action> _queue = new Queue<Action>();
        readonly int _throttle;
        int _currentQueueSize;

        public ThrottledQueue(int throttle = 6)
        {
            _throttle = throttle;
        }

        public void Enqueue(IEnumerable<Action> action)
        {
            lock (_queue)
            {
                foreach (var @do in action)
                    _queue.Enqueue(@do);
            }
        }

        public void Enqueue(Action action)
        {
            Enqueue(new[] { action });
        }

        public void Start()
        {
            for (int i = 0; i < _throttle; i++)
                TryStartOne();
        }

        public void WaitForCompletion()
        {
            while(true)
            {
                lock (_queue)
                {
                    if (_queue.Count == 0 && _currentQueueSize == 0)
                        return;
                }

                _finished.WaitOne();
            }
        }

        void TryStartOne()
        {
            lock (_queue)
            {
                if (_currentQueueSize >= _throttle)
                    return;
                _currentQueueSize++;
            }

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    Action action;
                    lock (_queue)
                    {
                        if (_queue.Count == 0)
                        {
                            _currentQueueSize--;
                            return;
                        }

                        action = _queue.Dequeue();
                    }

                    action();
                    lock (_queue)
                    {
                        _currentQueueSize--;
                        Start();
                        _finished.Set();
                    }
                }
                finally
                {
                    _finished.Set();
                }
            });
        }
    }
}