using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Configuration;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories
{
    public interface ISupportCaching
    {
        CacheState GetState();
        UpdateToken Update(UpdateToken lastToken);
        void Clear();
    }
    public class UpdateToken
    {
        readonly string _value;

        public UpdateToken(string value)
        {
            _value = value;
        }

        public string Value { get; private set; }
    }
    public class CacheState
    {
        public long CacheSize { get; set; }
        public UpdateToken UpdateToken { get; set; }
        public string FileName { get; set; }
    }
    public class PackageEntryContainer
    {
        public PackageEntryContainer()
        {
            Entries = new List<PackageEntry>();

        }
        public List<PackageEntry> Entries { get; set; }
    }
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

        public UpdateToken GetLastToken(string repositoryToken)
        {
            return GetCacheLocation(repositoryToken).UpdateToken;
        }
        public ILookup<string, IPackageInfo> LoadPackages(string repositoryToken, Func<PackageEntry,IPackageInfo> packageBuilder)
        {
            
            var cacheLocation = GetCacheLocation(repositoryToken);

            var entries = GetAllPackages(cacheLocation.FileName).Entries.Select(packageBuilder);

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
            if (!_index.CacheLocations.TryGetValue(repositoryToken, out state))
            {
                state = CreateRepositoryCache(repositoryToken);
            }
            return state;
        }

        CacheState CreateRepositoryCache(string repositoryToken)
        {
            var file = _repoCache.GetUniqueFile("cache");
            var cacheState = new CacheState { FileName = file.Name };
            _index.CacheLocations.Add(repositoryToken, cacheState);
            _configManager.Save(_index);
            return cacheState;
        }

        public void AppendPackages(string repositoryToken, UpdateToken newToken, IEnumerable<PackageEntry> packages)
        {
            var newPackages = packages.ToList();
            var cacheLocation = GetCacheLocation(repositoryToken);
            var oldPackages = GetAllPackages(cacheLocation.FileName);
            var packagesToUpdate = oldPackages.Entries.Where(old => newPackages.Any(@new => @new.Name.EqualsNoCase(old.Name) && @new.Version == old.Version));
            var newList = oldPackages.Entries.Except(packagesToUpdate).Concat(newPackages).ToList();
            // TODO: Make save retry in case theres a lock
            _configManager.Save(new PackageEntryContainer { Entries = newList }, new Uri(cacheLocation.FileName, UriKind.Relative));
            cacheLocation.UpdateToken = newToken;
            _configManager.Save(_index);
        }
    }
    [Path("cache-index")]
    public class CacheEntries
    {
        public CacheEntries()
        {
            CacheLocations = new Dictionary<string, CacheState>();
        }
        public IDictionary<string, CacheState> CacheLocations { get; set; }
    }
}