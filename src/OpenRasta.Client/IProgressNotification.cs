using System;

namespace OpenRasta.Client
{
    public interface IProgressNotification
    {
        event EventHandler<StatusChangedEventArgs> StatusChanged;
        event EventHandler<ProgressEventArgs> Progress;
    }
}