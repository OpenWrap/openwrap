using System;
using System.Collections.Generic;

namespace OpenWrap.Configuration
{
    [PathUri(ConstantUris.URI_REMOTES)]
    public class RemoteRepositories : Dictionary<string, RemoteRepository>
    {
        public static readonly RemoteRepositories Default =
                new RemoteRepositories
                {
                        { "openwrap", new RemoteRepository
                        {
                            FetchRepository = "[indexed]http://wraps.openwrap.org",
                            PublishRepositories = {"[indexed]http://wraps.openwrap.org"},

                        } 
                        }
                };

        public RemoteRepositories()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}