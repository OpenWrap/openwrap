using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public interface IRemoteManager
    {
        IEnumerable<IPackageRepository> FetchRepositories { get; }
        IEnumerable<IPackageRepository> PublishRepositories { get; }
    }
}