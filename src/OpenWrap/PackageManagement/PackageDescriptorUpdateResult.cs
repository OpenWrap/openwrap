using System;
using OpenWrap.Commands;
using OpenWrap.PackageModel;

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

    public class PackageDependencyRemovedResult : PackageOperationResult, ICommandOutput
    {
        public PackageDependencyRemovedResult(PackageDependency dependency)
        {
            Dependency = dependency;
        }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return new Info("Dependency '{0}' removed successfully.", Dependency);
        }

        public PackageDependency Dependency { get; set; }

        public CommandResultType Type
        {
            get { return CommandResultType.Info; }
        }
    }
    public class PackageDependencyAddedResult : PackageOperationResult
    {
        public PackageDependencyAddedResult(PackageDependency dependency)
        {
            Dependency = dependency;
        }

        public override bool Success
        {
            get { return true; }
        }

        public override ICommandOutput ToOutput()
        {
            return new Info("Dependency '{0}' added successfully.", Dependency);
        }

        public PackageDependency Dependency { get; set; }
    }
    public class PackageDependencyAlreadyExists : PackageOperationResult, ICommandOutput
    {
        public string PackageName { get; set; }

        public PackageDependencyAlreadyExists(string packageName)
        {
            PackageName = packageName;
        }

        public override bool Success
        {
            get { return false; }
        }

        public override ICommandOutput ToOutput()
        {
            return this;
        }

        public CommandResultType Type
        {
            get { return CommandResultType.Error; }
        }
        public override string ToString()
        {
            return
                string.Format(
                    "The package already exists or is locked. Use 'unlock-wrap {0}' to unlock, 'set-wrap {0} -version x.x.x' to change the version or 'update-wrap {0}' to update to a newer version.",
                    PackageName);
        }
    }
}