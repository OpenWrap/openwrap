using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace Tests.Commands.contexts
{
    public class remote_command<T> : command_context<T> where T : ICommand
    {
        readonly RemoteRepositories _repositories = new RemoteRepositories();

        protected override void when_executing_command(string parameters)
        {
            given_remote_configuration(_repositories);
            base.when_executing_command(parameters);
            ConfiguredRemotes = ServiceLocator.GetService<IConfigurationManager>().Load<RemoteRepositories>();
        }
        protected void given_remote_config(string name, string fetchToken = null, int? priority = null, params string[] publishTokens)
        {
            publishTokens = publishTokens ?? new string[0];

            _repositories.Add(new RemoteRepository
            {
                Name = name,
                Priority = priority ?? _repositories.Count +1,
                FetchRepository = { Token = fetchToken ?? "[memory]" + name },
                PublishRepositories = publishTokens.Select(x => new RemoteRepositoryEndpoint { Token = x }).ToList()
            });
        }
    }
}