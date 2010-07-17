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
    [Command(Noun="Remote", Verb="Add")]
    public class AddRemoteCommand : AbstractCommand
    {
        [CommandInput(Position=0, IsRequired=true)]
        public string Name { get; set; }

        [CommandInput(Position=1, IsRequired=true)]
        public Uri Href { get; set; }

        IConfigurationManager ConfigurationManager { get { return WrapServices.GetService<IConfigurationManager>(); } }
        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(InvalidName())
                    .Or(NameAlreadyExists())
                    .Or(AddRemote());
        }

        IEnumerable<ICommandOutput> AddRemote()
        {
            var repositories = ConfigurationManager.LoadRemoteRepositories();
            repositories[Name] = new RemoteRepository
            {
                    Href = Href,
                    Name = Name
            };
            ConfigurationManager.SaveRemoteRepositories(repositories);
            yield return new GenericMessage(string.Format("Remote repository '{0}' added.", Name));
        }

        IEnumerable<ICommandOutput> NameAlreadyExists()
        {
            if (ConfigurationManager.LoadRemoteRepositories().ContainsKey(Name))
                yield return new GenericError("A repository with the name '{0}' already exists.", Name);
        }

        IEnumerable<ICommandOutput> InvalidName()
        {
            if (!NameIsValid)
                yield return new GenericError("The 'Name' parameter is invalid.");
        }

        protected bool NameIsValid
        {
            get { return Regex.IsMatch(Name, @"^\S+$"); }
        }
    }
}
