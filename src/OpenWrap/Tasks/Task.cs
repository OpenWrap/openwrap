using System;
using OpenRasta.Client;

namespace OpenWrap.Tasks
{
    public class Task : ITask, ITaskChanges
    {
        readonly string _taskName;
        readonly string _taskDescription;
        readonly Action<ITaskChanges> _task;

        public Task(string taskName, string taskDescription, Action<ITaskChanges> task)
        {
            _taskName = taskName;
            _taskDescription = taskDescription;
            _task = task;
        }

        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<EventArgs> Complete;
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

        void NotifyProgressChanged(int i)
        {
        }

        public void Status(string status)
        {
            StatusChanged.Raise(this, new StatusChangedEventArgs(status));
        }

        public void Progress(int progress)
        {
            ProgressChanged.Raise(this, new ProgressEventArgs(progress));
        }
    }

}