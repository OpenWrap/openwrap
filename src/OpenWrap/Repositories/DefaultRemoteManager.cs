using System.Collections.Generic;
using System.Linq;
using System.Net;
using OpenWrap.Collections;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;

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

        public IEnumerable<IPackageRepository> FetchRepositories(string input = null)
        {
            var remotes = _configurationManager.Load<RemoteRepositories>();
            var configured = from config in GetRepositoriesOrdered(input, remotes)
                             let remote = FromToken(config.FetchRepository.Token)
                             where remote != null
                             select InjectAuthentication(remote, config.FetchRepository);

            return GetAmbient(input, remotes).Concat(configured).ToList();
        }

        public IEnumerable<IEnumerable<IPackageRepository>> PublishRepositories(string input = null)
        {
            var remotes = _configurationManager.Load<RemoteRepositories>();
            var configuredPublish = from config in GetRepositoriesOrdered(input, remotes)
                                    where config.PublishRepositories.Count > 0
                                    select from publish in config.PublishRepositories
                                           let remote = FromToken(publish.Token)
                                           where remote != null
                                           select remote;
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

        static IPackageRepository InjectAuthentication(IPackageRepository remote, RemoteRepositoryEndpoint config)
        {
            if (config.Username == null) return remote;
            var auth = remote.Feature<ISupportAuthentication>();
            if (auth == null) return null;

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
    }
}