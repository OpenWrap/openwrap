using System;
using System.Net;

namespace OpenWrap.Repositories
{
    public interface ISupportAuthentication : IRepositoryFeature
    {
        IDisposable WithCredentials(NetworkCredential credentials);
    }
}