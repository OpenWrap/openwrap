using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;

namespace OpenWrap.Commands.Wrap
{
    public static class PackageGraphExtensions
    {
        public static IEnumerable<IPackageInfo> AffectedPackages(this IEnumerable<IPackageInfo> allPackages, IEnumerable<IPackageInfo> rootPackages)
        {
            var packageNamesToLoad = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            new PackageGraphVisitor(allPackages).VisitFrom(
                rootPackages.Select(x => new PackageDependency(x.Name)),
                (from, dep, to) =>
                {
                    packageNamesToLoad.Add(to.Name);
                    return true;
                });
            return allPackages.Where(x => packageNamesToLoad.Contains(x.Name));
        }
    }
}