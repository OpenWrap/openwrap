using System;
using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public interface IRemoteRepositoryBuilder
    {
        IPackageRepository BuildPackageRepositoryForUri(string repositoryName, Uri repositoryHref);
        IEnumerable<IPackageRepository> GetConfiguredPackageRepositories();
    }
}