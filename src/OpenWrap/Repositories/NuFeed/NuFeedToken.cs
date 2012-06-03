using System;
using OpenWrap.Repositories.Caching;

namespace OpenWrap.Repositories.NuFeed
{
    public class NuFeedToken : UpdateToken
    {
        public NuFeedToken(DateTimeOffset lastUpdate)
            : base(lastUpdate.ToString("yyyy-MM-ddThh:mm:ss"))
        {
        }
    }
}