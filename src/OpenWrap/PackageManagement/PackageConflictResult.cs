using System;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageConflictResult : PackageResolveError
    {
        public PackageConflictResult(ResolvedPackage result)
                : base(result)
        {
        }

        public override bool Success
        {
            get { return false; }
        }

        public override ICommandOutput ToOutput()
        {
            var conflictingVersions = Package.DependencyStacks.Select(x => "\t" + x.ToString()).Join(Environment.NewLine);
            return new Error("Package {0} has conflicting versions:" + Environment.NewLine + conflictingVersions, Package.Identifier.Name);
        }
    }
}