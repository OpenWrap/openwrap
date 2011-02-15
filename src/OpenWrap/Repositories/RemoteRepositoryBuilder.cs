using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Configuration;
using OpenWrap.Repositories.FileSystem;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Repositories
{
    public class RemoteRepositoryBuilder : IRemoteRepositoryBuilder
    {
        readonly IConfigurationManager _configurationManager;
        readonly IFileSystem _fileSystem;

        public RemoteRepositoryBuilder()
                : this(Services.ServiceLocator.GetService<IFileSystem>(), Services.ServiceLocator.GetService<IConfigurationManager>())
        {
        }

        public RemoteRepositoryBuilder(IFileSystem fileSystem, IConfigurationManager configurationManager)
        {
            _fileSystem = fileSystem;
            _configurationManager = configurationManager;
        }

        public IPackageRepository BuildPackageRepositoryForUri(string repositoryName, Uri repositoryHref)
        {
            try
            {
                if (repositoryHref.Scheme.EqualsNoCase("nuget")
                    || repositoryHref.Scheme.EqualsNoCase("nupack"))
                {
                    var builder = new UriBuilder(repositoryHref);
                    builder.Scheme = "http";
                    return new HttpRepository(
                            _fileSystem,
                            repositoryName,
                            new NuGetFeedNavigator(builder.Uri));
                }
                if (repositoryHref.Scheme.EqualsNoCase("http") ||
                    repositoryHref.Scheme.EqualsNoCase("https"))
                    return new HttpRepository(_fileSystem, repositoryName, new HttpRepositoryNavigator(repositoryHref));
                if (repositoryHref.Scheme.EqualsNoCase("file"))
                    return new IndexedFolderRepository(repositoryName, _fileSystem.GetDirectory(repositoryHref.LocalPath));
            }
            catch
            {
            }
            return null;
        }

        public IEnumerable<IPackageRepository> GetConfiguredPackageRepositories()
        {
            return _configurationManager.LoadRemoteRepositories()
                    .OrderBy(x => x.Value.Priority)
                    .Select(x => BuildPackageRepositoryForUri(x.Key, x.Value.Href))
                    .Where(x => x != null);
        }
    }
}