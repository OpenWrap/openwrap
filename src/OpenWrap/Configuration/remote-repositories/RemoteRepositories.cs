using System;
using System.Collections.Generic;

namespace OpenWrap.Configuration
{
    public class RemoteRepositories : Dictionary<string, RemoteRepository>
    {
        public static readonly RemoteRepositories Default =
                new RemoteRepositories
                {
                        { "openwrap", new RemoteRepository { Href = new Uri("http://wraps.openwrap.org") } }
                };

        public RemoteRepositories() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}