using System;

namespace OpenWrap.Configuration.Remotes
{
    public class RemoteRepositoryEndpoint
    {
        [Encrypt]
        public string Password { get; set; }

        public string Token { get; set; }
        public string Username { get; set; }
    }
}