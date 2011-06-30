using System;

namespace OpenRasta.Client
{
    public interface IClientResponse : IResponse, IProgressNotification
    {
        Uri ResponseUri { get; }
    }
}