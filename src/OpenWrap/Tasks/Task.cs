using System;
using OpenRasta.Client;

namespace OpenWrap.Tasks
{
    public class Task : ITask, ITaskChanges
    {
        readonly Action<ITaskChanges> _task;
        readonly string _taskDescription;
        readonly string _taskName;

        public Task(string taskName, string taskDescription, Action<ITaskChanges> task)
        {
            _taskName = taskName;
            _taskDescription = taskDescription;
            _task = task;
        }

        public event EventHandler<EventArgs> Complete;

        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public void Run()
        {
            try
            {
                _task(this);
            }
            finally
            {
                Complete.Raise(this, EventArgs.Empty);
            }
        }

        public void Progress(int progress)
        {
            ProgressChanged.Raise(this, new ProgressEventArgs(progress));
        }

        public void Status(string status)
        {
            StatusChanged.Raise(this, new StatusChangedEventArgs(status));
        }

        void NotifyProgressChanged(int i)
        {
        }
    }
}