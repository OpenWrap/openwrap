using System;
using OpenWrap.Repositories.Caching;

namespace OpenWrap.Repositories
{
    public interface ISupportCaching : IRepositoryFeature
    {
        CacheState GetState();
        void Update();
        void Clear();
    }
}