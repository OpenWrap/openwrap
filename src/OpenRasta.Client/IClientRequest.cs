using System;

namespace OpenRasta.Client
{
    public interface IClientRequest : IRequest, IProgressNotification
    {
        IClientResponse Send();
    }
    public interface IProgressNotification
    {
        event EventHandler<StatusChangedEventArgs> StatusChanged;
        event EventHandler<ProgressEventArgs> Progress;
    }
}