using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public interface IRemoteManager
    {
        IEnumerable<IPackageRepository> FetchRepositories(string input);
        IEnumerable<IEnumerable<IPackageRepository>> PublishRepositories(string input);
    }
}