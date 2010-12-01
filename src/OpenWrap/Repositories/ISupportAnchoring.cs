using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public interface ISupportAnchoring : IPackageRepository
    {
        IEnumerable<IPackageInfo> AnchorPackages(IEnumerable<IPackageInfo> packagesToAnchor);
    }
}