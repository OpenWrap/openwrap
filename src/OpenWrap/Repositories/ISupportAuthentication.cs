using System;

namespace OpenWrap.Repositories
{
    public interface ISupportAuthentication
    {
        IDisposable WithCredentials(string user, string password);
    }
}