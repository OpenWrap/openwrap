using System;
using System.Collections.Generic;
using OpenWrap.Collections;

namespace OpenWrap.Configuration.Remotes
{
    [Path("remotes")]
    public class RemoteRepositories : IndexedDictionary<string, RemoteRepository>
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
            : base(_=>_.Name, (key,_)=>_.Name = key, StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}