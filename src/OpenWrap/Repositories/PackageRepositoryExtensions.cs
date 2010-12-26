using System;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public static class PackageRepositoryExtensions
    {
        public static bool HasPackage(this IPackageRepository packageRepository, string name, string version)
        {
            var typedVersion = new Version(version);
            return packageRepository.PackagesByName[name].Any(x => x.Version == typedVersion);
        }
    }
}