using System;
using OpenRasta.Client;

namespace OpenWrap.Tasks
{
    public interface IProgressOutput
    {
        event EventHandler<ProgressEventArgs> ProgressChanged;
        event EventHandler<StatusChangedEventArgs> StatusChanged;
        event EventHandler Complete;
    }
}