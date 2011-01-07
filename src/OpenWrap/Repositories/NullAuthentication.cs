using System;

namespace OpenWrap.Repositories
{
    public class NullAuthentication : ISupportAuthentication, IDisposable
    {
        public IDisposable WithCredentials(Credentials credentials)
        {
            return this;
        }

        public void Dispose()
        {
            
        }
    }
}