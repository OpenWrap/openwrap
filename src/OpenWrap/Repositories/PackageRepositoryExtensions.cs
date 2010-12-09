using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public static class PackageRepositoryExtensions
    {
        public static bool HasPackage(this IPackageRepository packageRepository, string name, string version)
        {
            var typedVersion = new Version(version);
            return packageRepository.PackagesByName[name].Any(x => x.Version == typedVersion);
        }
        public static bool HasDependency(this IPackageRepository packageRepository, string name, Version version)
        {
            return packageRepository.Find(
                new PackageDependencyBuilder(name).VersionVertex(new ExactVersionVertex(version))) != null;
        }
    }
}