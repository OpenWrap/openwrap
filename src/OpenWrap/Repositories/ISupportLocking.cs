using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface ISupportLocking : IRepositoryFeature
    {
        void Lock(string scope, IEnumerable<IPackageInfo> packages);
        ILookup<string, IPackageInfo> LockedPackages { get;  }
        void Unlock(string scope, IEnumerable<IPackageInfo> packages);
    }
}