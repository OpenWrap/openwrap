using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "list", IsDefault = true)]
    public class ListRemoteCommand : AbstractCommand
    {
        readonly IConfigurationManager _configurationManager;
        readonly IEnumerable<IRemoteRepositoryFactory> _factories;

        public ListRemoteCommand()
            : this(ServiceLocator.GetService<IConfigurationManager>(), ServiceLocator.GetService<IEnumerable<IRemoteRepositoryFactory>>())
        {
        }

        public ListRemoteCommand(IConfigurationManager config, IEnumerable<IRemoteRepositoryFactory> factories)
        {
            _configurationManager = config;
            _factories = factories;
        }

        [CommandInput]
        public bool Detailed { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            return _configurationManager.Load<RemoteRepositories>()
                .OrderBy(x => x.Value.Priority)
                .Select(CreateOutput);
        }

        RemoteEndpointData BuildEndpointData(RemoteRepositoryEndpoint value, string type)
        {
            var repo = Repo(value.Token);
            return new RemoteEndpointData(repo.Name, type, repo.Type, value.Token, value.Username, value.Password);
        }

        IEnumerable<RemoteEndpointData> BuildEndpoints(RemoteRepository value)
        {
            if (value.FetchRepository != null)
                yield return BuildEndpointData(value.FetchRepository, "fetch");

            foreach (var ep in value.PublishRepositories)
                yield return BuildEndpointData(ep, "publish");
        }

        ICommandOutput CreateOutput(KeyValuePair<string, RemoteRepository> x)
        {
            return Detailed == false
                       ? (ICommandOutput)new RemoteRepositoryData(x.Value.Priority, x.Value.Name, x.Value.PublishRepositories.Count > 0, x.Value.FetchRepository != null)
                       : new RemoteRepositoryDetailedData(x.Value.Priority, x.Value.Name, BuildEndpoints(x.Value));
        }

        IPackageRepository Repo(string token)
        {
            return _factories.Select(x => x.FromToken(token)).NotNull().FirstOrDefault();
        }
    }
}