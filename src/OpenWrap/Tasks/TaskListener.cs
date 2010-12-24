using System.Collections.Generic;
using System.Threading;

namespace OpenWrap.Tasks
{
    public class TaskListener : ITaskListener
    {
        readonly TaskManager _manager;
        readonly Queue<ITask> _queue = new Queue<ITask>();
        readonly EventWaitHandle _stop = new ManualResetEvent(false);
        readonly EventWaitHandle _sync = new AutoResetEvent(false);

        public TaskListener(TaskManager manager)
        {
            _manager = manager;
        }

        public IEnumerable<ITask> Start()
        {
            _manager.TaskStarted += HandleTaskStarted;
            do
            {
                var wait = WaitHandle.WaitAny(new[] { _sync, _stop });
                if (wait == 1) // is _stop
                    yield break;
                lock (_queue)
                {
                    while (_queue.Count > 0)
                        yield return _queue.Dequeue();
                }
            } while (true);
        }

        public void Stop()
        {
            _stop.Set();
            _manager.TaskStarted -= HandleTaskStarted;
        }

        void HandleTaskStarted(object source, TaskEventArgs e)
        {
            lock (_queue)
            {
                _queue.Enqueue(e.Task);
                _sync.Set();
            }
        }
    }
}