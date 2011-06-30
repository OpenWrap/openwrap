using System;
using OpenWrap.Commands;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageAddedResult : PackageOperationResult
    {
        public PackageAddedResult(IPackageInfo package, IPackageRepository repository)
        {
            Package = package;
            Repository = repository;
        }

        public IPackageInfo Package { get; set; }
        public IPackageRepository Repository { get; set; }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return new Info("{0}: {1} added.", Repository.Name, Package.Identifier);
        }
    }
}