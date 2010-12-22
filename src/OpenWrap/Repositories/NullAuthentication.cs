using System;

namespace OpenWrap.Repositories
{
    public class NullAuthentication : ISupportAuthentication, IDisposable
    {
        public IDisposable WithCredentials(string user, string password)
        {
            return this;
        }

        public void Dispose()
        {
            
        }
    }
}