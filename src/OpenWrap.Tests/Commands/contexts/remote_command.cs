using System;
using System.Collections.Generic;
using OpenWrap.Commands;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace Tests.Commands.contexts
{
    public class remote_command<T> : command_context<T> where T : ICommand
    {
        public remote_command()
        {

        }

        protected override void when_executing_command(string parameters)
        {
            base.when_executing_command(parameters);
            ConfiguredRemotes = ServiceLocator.GetService<IConfigurationManager>().Load<RemoteRepositories>();
        }
    }
}