using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public static class PackageRepositoryExtensions
    {
        public static bool HasPackage(this IPackageRepository packageRepository, string name, string version)
        {
            var typedVersion = SemanticVersion.TryParseExact(version);
            return packageRepository.PackagesByName[name].Any(x => x.SemanticVersion == typedVersion);
        }
        public static ILookup<string,IPackageInfo> Packages(this IEnumerable<IPackageRepository> repositories)
        {
            return (from repository in repositories
                    from packageByName in repository.PackagesByName
                    from package in packageByName
                    select new { key = packageByName.Key, package })
                    .ToLookup(_ => _.key, _ => _.package, StringComparer.OrdinalIgnoreCase);
        }
    }
}