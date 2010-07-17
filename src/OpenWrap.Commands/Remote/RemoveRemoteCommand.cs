using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenWrap.Configuration;
using OpenWrap.Configuration.remote_repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    // TODO: only load remote repository data at beginning of execution
    [Command(Noun="Remote", Verb="Remove")]
    public class RemoveRemoteCommand : AbstractCommand
    {
        [CommandInput(Position=0, IsRequired=true)]
        public string Name { get; set; }

        IConfigurationManager ConfigurationManager { get { return WrapServices.GetService<IConfigurationManager>(); } }
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
                yield return new GenericError("Remote repository '{0}' not found.", Name);
        }
    }
}
