using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using OpenWrap.Collections;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public class DefaultRemoteManager : IRemoteManager
    {
        readonly IConfigurationManager _configurationManager;
        readonly IEnumerable<IRemoteRepositoryFactory> _factories;

        public DefaultRemoteManager(IConfigurationManager configurationManager, IEnumerable<IRemoteRepositoryFactory> factories)
        {
            _configurationManager = configurationManager;
            _factories = factories;
        }
        // TODO: Write tests around name configuration of repositories
        public IEnumerable<IPackageRepository> FetchRepositories(string input = null)
        {
            var remotes = LoadRemotes();
            var configured = from config in GetRepositoriesOrdered(input, remotes)
                             let remote = FromToken(config.FetchRepository.Token)
                             where remote != null
                             select InjectConfigName(config,InjectAuthentication(remote, config.FetchRepository));

            return GetAmbient(input, remotes).Concat(configured).ToList();
        }

        public IEnumerable<IEnumerable<IPackageRepository>> PublishRepositories(string input = null)
        {
            var remotes = LoadRemotes();
            var configuredPublish = from config in GetRepositoriesOrdered(input, remotes)
                                    where config.PublishRepositories.Count > 0
                                    select from publish in config.PublishRepositories
                                           let remote = FromToken(publish.Token)
                                           where remote != null
                                           select InjectConfigName(config,InjectAuthentication(remote, publish));
            var ambient = GetAmbient(input, remotes).FirstOrDefault();
            return ambient != null
                       ? new[] { new[] { ambient } }.Concat(configuredPublish).ToList()
                       : configuredPublish.ToList();
        }

        static IEnumerable<RemoteRepository> GetRepositoriesOrdered(string input, RemoteRepositories remoteRepositories)
        {
            var remotes = remoteRepositories;
            IEnumerable<RemoteRepository> entries = remotes.Select(x => x.Value).OrderBy(x => x.Priority);
            if (input != null && remotes.ContainsKey(input))
            {
                entries = new[] { remotes[input] }.Concat(entries.Where(_ => _ != remotes[input]));
            }
            return entries;
        }
        static IPackageRepository InjectConfigName(RemoteRepository config, IPackageRepository endpoint)
        {
            return new NameTaggedRepository(endpoint, name => 
                config.Name.EqualsNoCase(name) ? name : string.Format("{0} - {1}", config.Name, name));
        }
        static IPackageRepository InjectAuthentication(IPackageRepository remote, RemoteRepositoryEndpoint config)
        {
            if (config.Username == null) return remote;
            var auth = remote.Feature<ISupportAuthentication>();
            if (auth == null) return remote;

            return new PreAuthenticatedRepository(remote, auth, new NetworkCredential(config.Username, config.Password));
        }

        IPackageRepository FromInput(string token)
        {
            return _factories.Select(x => x.FromUserInput(token)).NotNull().FirstOrDefault();
        }

        IPackageRepository FromToken(string token)
        {
            return _factories.Select(x => x.FromToken(token)).NotNull().FirstOrDefault();
        }

        IEnumerable<IPackageRepository> GetAmbient(string input, RemoteRepositories remotes)
        {
            if (input != null && remotes.ContainsKey(input) == false)
            {
                var ambient = FromInput(input);
                if (ambient != null) yield return ambient;
            }
        }

        RemoteRepositories LoadRemotes()
        {
            var remotes = _configurationManager.Load<RemoteRepositories>();
            if (ReferenceEquals(remotes, RemoteRepositories.Default))
            {
                var legacyRemotes = _configurationManager.Load<Configuration.Remotes.Legacy.RemoteRepositories>();
                if (legacyRemotes != null)
                {
                    remotes.Clear();
                    foreach (var legacy in legacyRemotes)
                    {
                        var repo = FromInput(legacy.Value.Href.ToString());
                        if (repo == null) continue;
                        var newRemote = new RemoteRepository
                        {
                            Name = legacy.Key,
                            Priority = legacy.Value.Priority,
                            FetchRepository = new RemoteRepositoryEndpoint { Token = repo.Token }
                        };
                        if (repo.Feature<ISupportPublishing>() != null)
                            newRemote.PublishRepositories.Add(new RemoteRepositoryEndpoint { Token = repo.Token });
                        remotes.Add(newRemote);
                    }
                    _configurationManager.Save(remotes);
                }
            }
            return remotes;
        }
    }

    public class NameTaggedRepository : IPackageRepository
    {
        readonly IPackageRepository _actualRepo;
        readonly Func<string, string> _nameTag;

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { return _actualRepo.PackagesByName; }
        }

        public void RefreshPackages()
        {
            _actualRepo.RefreshPackages();
        }

        public string Name
        {
            get { return _nameTag(_actualRepo.Name); }
        }

        public string Token
        {
            get { return _actualRepo.Token; }
        }

        public string Type
        {
            get { return _actualRepo.Type; }
        }

        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            return _actualRepo.Feature<TFeature>();
        }

        public NameTaggedRepository(IPackageRepository actualRepo, Func<string,string> nameTag)
        {
            _actualRepo = actualRepo;
            _nameTag = nameTag;
        }
    }

}