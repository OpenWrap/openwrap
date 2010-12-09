using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public interface ISupportCleaning : IPackageRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packagesToKeep"></param>
        /// <returns>The packages that were removed from the repository</returns>
        IEnumerable<PackageCleanResult> Clean(IEnumerable<IPackageInfo> packagesToKeep);
    }
}