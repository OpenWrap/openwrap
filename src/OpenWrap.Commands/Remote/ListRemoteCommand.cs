using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Configuration;
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
            return ConfigurationManager.LoadRemoteRepositories()
                    .OrderBy(x => x.Value.Priority)
                    .Select(x => new RemoteRepositoryMessage(this, x.Key, x.Value))
                    .Cast<ICommandOutput>();
        }
    }
}