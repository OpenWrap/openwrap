using System.Collections.Generic;

namespace OpenWrap.Tasks
{
    public interface ITaskListener
    {
        IEnumerable<ITask> Start();
        void Stop();
    }
}