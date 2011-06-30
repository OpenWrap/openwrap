using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public interface IRemoteManager
    {
        IEnumerable<IPackageRepository> FetchRepositories(string input = null);
        IEnumerable<IEnumerable<IPackageRepository>> PublishRepositories(string input = null);
    }
}