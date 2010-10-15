using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public interface IPackageRepository
    {
        ILookup<string, IPackageInfo> PackagesByName { get; }
        IPackageInfo Find(PackageDependency dependency);
        
        void Refresh();
        string Name { get; }
    }
    public interface ISupportPublishing : IPackageRepository
    {
        IPackageInfo Publish(string packageFileName, Stream packageStream);
    }
    public interface ISupportCleaning : IPackageRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packagesToKeep"></param>
        /// <returns>The packages that were removed from the repository</returns>
        IEnumerable<IPackageInfo> Clean(IEnumerable<IPackageInfo> packagesToKeep);
    }

    public interface ISupportAnchoring : IPackageRepository
    {
        IEnumerable<IPackageInfo> VerifyAnchors(IEnumerable<IPackageInfo> packagesToAnchor);
    }

    public interface ISupportNuking : IPackageRepository
    {
        void Nuke(IPackageInfo packageInfo);
    }
}