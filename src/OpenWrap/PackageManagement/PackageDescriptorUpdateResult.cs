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
        public override ICommandOutput ToOutput()
        {
            if (Result == PackageDescriptorDependencyUpdate.Added)
                return new Info("Package added to the descriptor.");
            if (Result == PackageDescriptorDependencyUpdate.Removed)
                return new Info("Package removed from the descriptor.");
            if (Result == PackageDescriptorDependencyUpdate.Updated)
                return new Info("Package version updated in the descriptor.");
            if (Result == PackageDescriptorDependencyUpdate.DependencyNotFound)
                return new Error("Could not find a package to remove from the descriptor");
            return null;

        }
    }
}