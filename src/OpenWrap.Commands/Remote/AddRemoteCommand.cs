using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Configuration;


namespace OpenWrap.Commands.Remote
{
    [Command(Noun="remote", Verb="add")]
    public class AddRemoteCommand : AbstractCommand
    {
        int? _position;

        [CommandInput(Position=0, IsRequired=true)]
        public string Name { get; set; }

        [CommandInput(Position=1, IsRequired=true)]
        public Uri Href { get; set; }

        public int Position
        {
            get { return _position ?? 1; }
            set { _position = value; }
        }

        IConfigurationManager ConfigurationManager { get { return Services.ServiceLocator.GetService<IConfigurationManager>(); } }
        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(InvalidName())
                    .Or(NameAlreadyExists())
                    .Or(AddRemote());
        }

        IEnumerable<ICommandOutput> AddRemote()
        {
            var repositories = ConfigurationManager.LoadRemoteRepositories();
            int position = GetNewRemotePosition(repositories); 
            repositories[Name] = new RemoteRepository
            {
                Href = Href,
                Name = Name,
                Priority = position
            };
            ConfigurationManager.SaveRemoteRepositories(repositories);
            yield return new GenericMessage(string.Format("Remote repository '{0}' added.", Name));
        }

        int GetNewRemotePosition(RemoteRepositories repositories)
        {
            if (_position.HasValue)
            {
                return _position.Value;
            }
            else if (repositories.Count > 0)
            {
                return repositories.Values.Max(r => r.Priority) + 1;
            }
            else
            {
                return 1;
            }
        }

        IEnumerable<ICommandOutput> NameAlreadyExists()
        {
            if (ConfigurationManager.LoadRemoteRepositories().ContainsKey(Name))
                yield return new Error("A repository with the name '{0}' already exists.", Name);
        }

        IEnumerable<ICommandOutput> InvalidName()
        {
            if (!NameIsValid)
                yield return new Error("The 'Name' parameter is invalid.");
        }

        protected bool NameIsValid
        {
            get { return Regex.IsMatch(Name, @"^\S+$"); }
        }
    }
}
