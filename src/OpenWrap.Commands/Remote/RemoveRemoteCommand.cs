using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenWrap.Configuration;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun="remote", Verb="remove")]
    public class RemoveRemoteCommand : AbstractCommand
    {
        [CommandInput(Position=0, IsRequired=true)]
        public string Name { get; set; }

        IConfigurationManager ConfigurationManager { get { return Services.Services.GetService<IConfigurationManager>(); } }
        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(NameDoesntExist())
                    .Or(RemoveRemote());
        }

        IEnumerable<ICommandOutput> RemoveRemote()
        {
            var repositories = ConfigurationManager.LoadRemoteRepositories();
            
            repositories.Remove(Name);

            ConfigurationManager.SaveRemoteRepositories(repositories);
            yield return new GenericMessage(string.Format("Repository '{0}' removed.", Name));
        }

        IEnumerable<ICommandOutput> NameDoesntExist()
        {
            if (!ConfigurationManager.LoadRemoteRepositories().ContainsKey(Name))
                yield return new Error("Remote repository '{0}' not found.", Name);
        }
    }
}
