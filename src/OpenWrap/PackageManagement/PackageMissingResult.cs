using System;
using OpenWrap.Commands;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageMissingResult : PackageResolveError, ICommandOutput
    {
        public PackageMissingResult(ResolvedPackage result)
                : base(result)
        {
            Type = CommandResultType.Error;
        }

        public override bool Success
        {
            get { return false; }
        }

        public override ICommandOutput ToOutput()
        {
            return this;
        }

        public CommandResultType Type { get; set; }

        public override string ToString()
        {
            return string.Format("Package {0} not found in any repository.", Package.Identifier);
        }
    }
}