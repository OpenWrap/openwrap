using System.Collections.Generic;
using OpenWrap.PackageManagement;

namespace OpenWrap.Repositories
{
    public interface ISupportAnchoring : IPackageRepository
    {
        IEnumerable<PackageAnchoredResult> AnchorPackages(IEnumerable<IPackageInfo> packagesToAnchor);
    }
}