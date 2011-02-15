using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Configuration;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun="remote", Verb="list")]
    public class ListRemoteCommand : AbstractCommand
    {
        IConfigurationManager ConfigurationManager { get { return Services.ServiceLocator.GetService<IConfigurationManager>(); } }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return ConfigurationManager.LoadRemoteRepositories()
                    .OrderBy(x => x.Value.Priority)
                    .Select(x => new RemoteRepositoryMessage(this, x.Key, x.Value))
                    .Cast<ICommandOutput>();
        }
    }
}
