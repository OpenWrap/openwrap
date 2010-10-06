using System.IO;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public interface IPackageRepository
    {
        ILookup<string, IPackageInfo> PackagesByName { get; }
        IPackageInfo Find(PackageDependency dependency);
        IPackageInfo Publish(string packageFileName, Stream packageStream);
        bool CanPublish { get; }
        void Refresh();
        string Name { get; }

        bool CanDelete { get; }
        void Delete(IPackageInfo packageInfo);
    }
}