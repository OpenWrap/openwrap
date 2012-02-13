using System;
using System.Net;

namespace OpenWrap.Repositories
{
    public interface ISupportAuthentication : IRepositoryFeature
    {
        // TODO: Split breaking change in another interface
        IDisposable WithCredentials(NetworkCredential credentials);
        NetworkCredential CurrentCredentials { get; }
    }
}