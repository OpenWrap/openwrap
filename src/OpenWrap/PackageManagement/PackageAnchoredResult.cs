using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageAnchoredResult : PackageOperationResult
    {
        readonly ISupportAnchoring _repository;
        readonly IPackageInfo _package;
        readonly bool _success;

        public PackageAnchoredResult(ISupportAnchoring repository, IPackageInfo package, bool success)
        {
            _repository = repository;
            _package = package;
            _success = success;
        }

        public override ICommandOutput ToOutput()
        {
            if (_success)
                return new Warning("Package {0} could not be anchored in {1}.", _package.Identifier, _repository.Name);
            return new Info("Package {0} anchored in {1}.", _package.Identifier, _repository.Name);
        }
    }
}