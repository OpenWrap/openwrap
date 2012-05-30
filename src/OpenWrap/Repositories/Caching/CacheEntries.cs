using System.Collections.Generic;
using OpenWrap.Configuration;

namespace OpenWrap.Repositories.Caching
{
    [Path("cache-index")]
    public class CacheEntries : Dictionary<string, CacheState>
    {
    }
}