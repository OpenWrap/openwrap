using System;
using OpenWrap.Commands;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageMissingResult : PackageResolveError
    {
        public PackageMissingResult(ResolvedPackage result)
                : base(result)
        {
        }

        public override bool Success
        {
            get { return false; }
        }

        public override ICommandOutput ToOutput()
        {
            return new Error("Package {0} not found in repositories.", Package.Identifier);
        }
    }
}