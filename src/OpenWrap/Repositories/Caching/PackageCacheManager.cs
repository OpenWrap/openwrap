using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Configuration;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.Caching
{
    public class PackageCacheManager
    {
        readonly IDirectory _repoCache;
        CacheEntries _index;
        DefaultConfigurationManager _configManager;

        public PackageCacheManager(IDirectory repoCache)
        {
            _repoCache = repoCache;
            _configManager = new DefaultConfigurationManager(repoCache);
            _index = _configManager.Load<CacheEntries>() ?? new CacheEntries();
        }

        public CacheState GetState(string repositoryToken)
        {
            return GetCacheLocation(repositoryToken);
        }
        public ILookup<string, IPackageInfo> LoadPackages(string repositoryToken, Func<PackageEntry,IPackageInfo> packageBuilder)
        {
            
            var cacheLocation = GetCacheLocation(repositoryToken);

            var entries = GetAllPackages(cacheLocation.FileName).Packages.Select(packageBuilder);

            return entries.ToLookup(_ => _.Name, StringComparer.OrdinalIgnoreCase);
        }

        PackageEntryContainer GetAllPackages(string fileName)
        {
            return _configManager.Load<PackageEntryContainer>(new Uri(fileName, UriKind.Relative))
                   ?? new PackageEntryContainer();
        }

        CacheState GetCacheLocation(string repositoryToken)
        {
            CacheState state;
            if (!_index.TryGetValue(repositoryToken, out state))
            {
                state = CreateRepositoryCache(repositoryToken);
            }
            return state;
        }

        CacheState CreateRepositoryCache(string repositoryToken)
        {
            var file = _repoCache.GetUniqueFile("cache");
            var cacheState = new CacheState { FileName = file.Name };
            _index.Add(repositoryToken, cacheState);
            _configManager.Save(_index);
            return cacheState;
        }

        public void AppendPackages(string repositoryToken, UpdateToken newToken, IEnumerable<PackageEntry> packages)
        {
            if (newToken == null) throw new ArgumentNullException("newToken");
            var newPackages = packages.ToList();
            var cacheLocation = GetCacheLocation(repositoryToken);
            var oldPackages = GetAllPackages(cacheLocation.FileName);
            var packagesToUpdate = oldPackages.Packages.Where(old => newPackages.Any(@new => @new.Name.EqualsNoCase(old.Name) && @new.Version == old.Version));
            var newList = oldPackages.Packages.Except(packagesToUpdate).Concat(newPackages).ToList();

            // TODO: Make save retry in case theres a lock
            _configManager.Save(new PackageEntryContainer { Packages = newList }, new Uri(cacheLocation.FileName, UriKind.Relative));
            cacheLocation.UpdateToken = newToken;
            _configManager.Save(_index);
        }
    }
}