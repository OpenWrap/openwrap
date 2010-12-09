using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Services;

namespace OpenWrap.Repositories
{
    public static class PackageResolverExtensions
    {
        public static IEnumerable<IPackageInfo> LatestVersions(this IEnumerable<IPackageInfo> packages)
        {
            var packagesByVersion = packages
                    .NotNull()
                    .GroupBy(x => x.Identifier)
                    .OrderByDescending(x => x.Key.Version);
            return packagesByVersion
                    .Where(x=>x.All(p=>p.Nuked) == false)
                    .DefaultIfEmpty(packagesByVersion.FirstOrDefault())
                    .First() ?? Enumerable.Empty<IPackageInfo>();
        }
    }
}