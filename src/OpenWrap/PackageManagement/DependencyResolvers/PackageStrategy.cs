using System;
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

        static void Main(string[] args)
        {
            var semver = new[] { new SemanticVersion(1, 0, 2, build: "3"), new SemanticVersion(1, 0, 2, build:"4") };
            Console.WriteLine(semver.OrderByDescending(_=>_).First().ToString());
        }
    }
}