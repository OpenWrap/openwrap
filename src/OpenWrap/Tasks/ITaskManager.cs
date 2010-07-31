using System;

namespace OpenWrap.Tasks
{
    public interface ITaskManager
    {
        event EventHandler<TaskEventArgs> TaskStarted;
        void Run(string taskName, string taskDescription, Action<ITaskChanges> taskAction);
        ITaskListener GetListener();
    }
}