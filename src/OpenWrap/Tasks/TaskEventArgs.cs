using System;

namespace OpenWrap.Tasks
{
    public class TaskEventArgs : EventArgs
    {
        public TaskEventArgs(ITask task)
        {
            Task = task;
        }

        public ITask Task { get; set; }
    }
}