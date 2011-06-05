using System;
using System.Collections.Generic;

namespace OpenWrap.Configuration.Remotes.Legacy
{
    [Path("remote-repositories")]
    public class RemoteRepositories : Dictionary<string, RemoteRepository>
    {
        public RemoteRepositories()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}