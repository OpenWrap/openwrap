using System;
using System.Collections.Generic;
using OpenWrap.Commands.Errors;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration.Remotes;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "remove")]
    public class RemoveRemoteCommand : AbstractRemoteCommand
    {
        RemoteRepositories _remotes;

        [CommandInput(Position = 0, IsRequired = true)]
        public string Name { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            _remotes.Remove(Name);

            ConfigurationManager.Save(_remotes);
            yield return new RemoteRemoved(Name);
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return NameExists;
        }


        IEnumerable<ICommandOutput> NameExists()
        {
            _remotes = ConfigurationManager.Load<RemoteRepositories>();
            if (!_remotes.ContainsKey(Name))
                yield return new UnknownRemoteName(Name);
        }
    }
}