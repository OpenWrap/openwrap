using System;

namespace OpenWrap.Repositories
{
    public interface ISupportAuthentication
    {
        IDisposable WithCredentials(CredentialInfo credentials);
    }

    public class CredentialInfo
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }

        public CredentialInfo(string user, string password)
        {
            UserName = user;
            Password = password;
        }
    }
}