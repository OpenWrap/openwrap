using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public class RemoteAuthenticatioNotSupported : Error
    {
        public IPackageRepository Repository { get; set; }

        public RemoteAuthenticatioNotSupported(IPackageRepository repository)
            : base("Remote repository '{0}' does not support authentication.", repository.Name)
        {
            Repository = repository;
        }
    }
}