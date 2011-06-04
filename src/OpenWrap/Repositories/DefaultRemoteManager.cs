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

        public IEnumerable<IPackageRepository> FetchRepositories
        {
            get
            {
                return from config in _configurationManager.Load<RemoteRepositories>()
                       let remote = _factories.Select(x => x.FromToken(config.Value.FetchRepository.Token)).NotNull().FirstOrDefault()
                       where remote != null
                       select InjectAuthentication(remote, config.Value.FetchRepository);
            }
        }

        static IPackageRepository InjectAuthentication(IPackageRepository remote, RemoteRepositoryEndpoint config)
        {
            if (config.Username == null) return remote;
            var auth = remote.Feature<ISupportAuthentication>();
            if (auth == null) return null;

            return new PreauthenticatedRepository(remote, auth, new NetworkCredential(config.Username, config.Password));
        }

        public IEnumerable<IPackageRepository> PublishRepositories
        {
            get
            {
                return from config in _configurationManager.Load<RemoteRepositories>()
                       from publish in config.Value.PublishRepositories
                       let remote = _factories.Select(x => x.FromToken(publish.Token)).NotNull().FirstOrDefault()
                       where remote != null
                       select remote;
            }
        }
    }
}