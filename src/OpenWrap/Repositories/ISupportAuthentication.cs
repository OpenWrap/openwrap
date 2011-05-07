using System;

namespace OpenWrap.Repositories
{
    public interface ISupportAuthentication : IRepositoryFeature
    {
        IDisposable WithCredentials(Credentials credentials);
    }

    public class Credentials
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public Credentials(string user, string password)
        {
            Username = user;
            Password = password;
        }
    }
}