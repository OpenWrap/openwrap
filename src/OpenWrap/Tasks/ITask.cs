using System;
using OpenRasta.Client;

namespace OpenWrap.Tasks
{
    public interface ITask
    {
        event EventHandler<ProgressEventArgs> ProgressChanged;
        event EventHandler<EventArgs> Complete;
        void Run();
        event EventHandler<StatusChangedEventArgs> StatusChanged;
    }
}