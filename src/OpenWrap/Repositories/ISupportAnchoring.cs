using System.Collections.Generic;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface ISupportAnchoring : IPackageRepository
    {
        IEnumerable<PackageAnchoredResult> AnchorPackages(IEnumerable<IPackageInfo> packagesToAnchor);
    }
}