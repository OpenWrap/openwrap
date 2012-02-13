using System;
using System.Net;

namespace OpenWrap.Repositories
{
    public class NullAuthentication : ISupportAuthentication, IDisposable
    {
        public IDisposable WithCredentials(NetworkCredential credentials)
        {
            return this;
        }

        public NetworkCredential CurrentCredentials
        {
            get { return null; }
        }

        public void Dispose()
        {
            
        }
    }
}