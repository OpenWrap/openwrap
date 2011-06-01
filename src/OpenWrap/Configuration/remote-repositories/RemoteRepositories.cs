using System;
using System.Collections.Generic;

namespace OpenWrap.Configuration
{
    [Path("remotes")]
    public class RemoteRepositories : Dictionary<string, RemoteRepository>
    {
        public static readonly RemoteRepositories Default =
            new RemoteRepositories
            {
                {
                    "openwrap", new RemoteRepository
                    {
                        FetchRepository = { Token = "[indexed]http://wraps.openwrap.org" },
                        PublishRepositories = { new RemoteRepositoryEndpoint { Token = "[indexed]http://wraps.openwrap.org" } },
                    }
                }
            };

        public RemoteRepositories()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}