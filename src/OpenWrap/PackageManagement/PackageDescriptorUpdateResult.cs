using System;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement
{
    public class PackageDescriptorUpdateResult : PackageOperationResult
    {
        public PackageDescriptorUpdateResult(PackageDescriptorDependencyUpdate result)
        {
            Result = result;
        }

        public PackageDescriptorDependencyUpdate Result { get; private set; }

        public override bool Success
        {
            get { return Result == PackageDescriptorDependencyUpdate.Added || Result == PackageDescriptorDependencyUpdate.Updated; }
        }

        public override ICommandOutput ToOutput()
        {
            if (Result == PackageDescriptorDependencyUpdate.Added)
                return new Info("Trying to add package.");
            if (Result == PackageDescriptorDependencyUpdate.Removed)
                return new Info("Package removed from the descriptor.");
            if (Result == PackageDescriptorDependencyUpdate.Updated)
                return new Info("Trying to update package.");
            if (Result == PackageDescriptorDependencyUpdate.DependencyNotFound)
                return new Error("Could not find a package to remove from the descriptor.");
            return null;
        }
    }
}