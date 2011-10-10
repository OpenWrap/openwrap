using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public class LockingNotSupported : Error
    {
        public IPackageRepository Repository { get; set; }

        public LockingNotSupported(IPackageRepository repository) : base("The repository '{0}' does not support locking.", repository.Name)
        {
            Repository = repository;
        }
    }
}