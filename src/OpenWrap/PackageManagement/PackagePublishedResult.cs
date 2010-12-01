using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackagePublishedResult : PackageOperationResult
    {
        public PackagePublishedResult(IPackageInfo package, ISupportPublishing repository)
        {
            Package = package;
            Repository = repository;
        }

        public IPackageInfo Package { get; set; }
        public ISupportPublishing Repository { get; set; }
        public override ICommandOutput ToOutput()
        {
            return new Info("{0}: {1} published.", Repository.Name, Package.Identifier);
        }
    }
}