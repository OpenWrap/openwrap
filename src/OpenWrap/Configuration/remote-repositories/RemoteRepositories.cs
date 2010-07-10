using System;
using System.Collections.Generic;


namespace OpenWrap.Configuration.remote_repositories
{
    public class RemoteRepositories : Dictionary<string, RemoteRepository>
    {
        public RemoteRepositories() : base(StringComparer.OrdinalIgnoreCase)
        {
            
        }
        public static readonly RemoteRepositories Default =
                new RemoteRepositories
                {
                        { "openwrap", new RemoteRepository { Href = new Uri("http://wraps.openwrap.org") } }
                };
    }
}