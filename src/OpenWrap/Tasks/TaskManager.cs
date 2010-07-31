using System;
using System.Linq;
using System.Text;
using OpenWrap.Services;

namespace OpenWrap.Tasks
{
    public class TaskManager : ITaskManager
    {
        public static ITaskManager Instance
        {
            get
            {
                WrapServices.TryRegisterService<ITaskManager>(() => new TaskManager());
                return WrapServices.GetService<ITaskManager>();
            }
        }
        public event EventHandler<TaskEventArgs> TaskStarted;


        public void Run(string taskName, string taskDescription, Action<ITaskChanges> taskAction)
        {
            var task = new Task(taskName, taskDescription, taskAction);
            TaskStarted.Raise(this, new TaskEventArgs(task));
            task.Run();
        }
        public ITaskListener GetListener()
        {
            return new TaskListener(this);
        }
    }
}
