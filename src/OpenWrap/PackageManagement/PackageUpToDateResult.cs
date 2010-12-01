using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    internal class PackageUpToDateResult : PackageOperationResult
    {
        readonly IPackageInfo _existingUpToDateVersion;
        readonly ISupportPublishing _repository;

        public PackageUpToDateResult(IPackageInfo existingUpToDateVersion, ISupportPublishing repository)
        {
            _existingUpToDateVersion = existingUpToDateVersion;
            _repository = repository;
        }

        public override ICommandOutput ToOutput()
        {
            return new Info("{0}: {1} up-to-date.", _repository.Name, _existingUpToDateVersion.Identifier.Name);
        }
    }
}