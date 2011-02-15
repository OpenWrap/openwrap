using System;

namespace OpenWrap.Tasks
{
    public class TaskManager : ITaskManager
    {
        public event EventHandler<TaskEventArgs> TaskStarted;

        public static ITaskManager Instance
        {
            get
            {
                Services.ServiceLocator.TryRegisterService<ITaskManager>(() => new TaskManager());
                return Services.ServiceLocator.GetService<ITaskManager>();
            }
        }


        public ITaskListener GetListener()
        {
            return new TaskListener(this);
        }

        public void Run(string taskName, string taskDescription, Action<ITaskChanges> taskAction)
        {
            var task = new Task(taskName, taskDescription, taskAction);
            TaskStarted.Raise(this, new TaskEventArgs(task));
            task.Run();
        }
    }
}