using System.Collections.Generic;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface ISupportCleaning : IRepositoryFeature
    {
        /// <summary>
        /// </summary>
        /// <param name = "packagesToKeep"></param>
        /// <returns>The packages that were removed from the repository</returns>
        IEnumerable<PackageCleanResult> Clean(IEnumerable<IPackageInfo> packagesToKeep);
    }
}