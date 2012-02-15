using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public static class PackageStrategy
    {
        public static IEnumerable<IPackageInfo> Latest(IEnumerable<IPackageInfo> arg)
        {
#pragma warning disable 612,618
            return arg.Where(_ => _.SemanticVersion != null)
                .OrderByDescending(_ => _.SemanticVersion)
                .Concat(
                    arg.Where(_ => _.SemanticVersion == null && _.Version != null)
                        .OrderByDescending(_ => _.Version)
                );
#pragma warning restore 612,618
        }
    }
}