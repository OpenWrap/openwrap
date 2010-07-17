using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenWrap.Tasks
{
    public class TaskManager
    {
        public event EventHandler<TaskEventArgs> TaskStarted;

        public void Run(string taskName, string taskDescription, Action<Action<int>> taskAction)
        {
            var task = new Task(taskName, taskDescription, taskAction);
            TaskStarted.Raise(this, new TaskEventArgs(task));
            task.Run();
        }
        public ITaskManagerListener GetListener()
        {
            return new TaskManagerListener(this);
        }
    }

    public interface ITaskManagerListener
    {
        IEnumerable<ITask> Start();
        void Stop();
    }

    public class TaskManagerListener : ITaskManagerListener
    {
        readonly TaskManager _manager;
        readonly Queue<ITask> _queue = new Queue<ITask>();
        readonly EventWaitHandle _sync = new AutoResetEvent(false);
        readonly EventWaitHandle _stop = new ManualResetEvent(false);

        public TaskManagerListener(TaskManager manager)
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

        void HandleTaskStarted(object source, TaskEventArgs e)
        {
            lock (_queue)
            {
                _queue.Enqueue(e.Task);
                _sync.Set();
            }

        }

        public void Stop()
        {
            _stop.Set();
            _manager.TaskStarted -= HandleTaskStarted;
        }
    }
    public class TaskEventArgs : EventArgs
    {
        public TaskEventArgs(ITask task)
        {
            Task = task;
        }

        public ITask Task { get; set; }
    }
    public class Task : ITask
    {
        readonly string _taskName;
        readonly string _taskDescription;
        readonly Action<Action<int>> _task;

        public Task(string taskName, string taskDescription, Action<Action<int>> task)
        {
            _taskName = taskName;
            _taskDescription = taskDescription;
            _task = task;
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        public event EventHandler<EventArgs> Complete;
        public void Run()
        {
            _task(NotifyProgressChanged);
        }

        void NotifyProgressChanged(int i)
        {
            ProgressChanged.Raise(this, new ProgressChangedEventArgs(i, null));
        }
    }

    public interface ITask
    {
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        event EventHandler<EventArgs> Complete;
        void Run();
    }
}
