using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "list", IsDefault = true)]
    public class ListRemoteCommand : AbstractCommand
    {
        IConfigurationManager ConfigurationManager
        {
            get { return ServiceLocator.GetService<IConfigurationManager>(); }
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            return ConfigurationManager.Load<RemoteRepositories>()
                    .OrderBy(x => x.Value.Priority)
                    .Select(x => new RemoteRepositoryInfo(x.Key, x.Value))
                    .Cast<ICommandOutput>();
        }
    }
}