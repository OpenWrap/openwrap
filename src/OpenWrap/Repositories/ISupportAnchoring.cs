using System.Collections.Generic;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface ISupportAnchoring : IRepositoryFeature
    {
        IEnumerable<PackageAnchoredResult> AnchorPackages(IEnumerable<IPackageInfo> packagesToAnchor);
    }
}