using System;
using System.Net;

namespace OpenRasta.Client
{
    public interface IClientRequest : IRequest, IProgressNotification
    {
        IClientResponse Send();
        NetworkCredential Credentials { get; set; }
        void RegisterHandler(Func<IClientResponse, bool> predicate, Action<IClientResponse> handler);
    }
}