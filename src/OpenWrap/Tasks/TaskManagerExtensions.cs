using System;

namespace OpenWrap.Tasks
{
    public static class TaskManagerExtensions
    {
        public static void Run(this ITaskManager manager, string taskName, Action<ITaskChanges> taskChanges)
        {
            manager.Run(taskName, string.Empty, taskChanges);
        }
    }
}