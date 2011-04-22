using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Configuration;
using OpenWrap.Services;


namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "add")]
    public class AddRemoteCommand : AbstractCommand
    {
        int? _priority;

        [CommandInput(Position = 1, IsRequired = true)]
        public Uri Href { get; set; }

        [CommandInput(Position = 0, IsRequired = true)]
        public string Name { get; set; }

        [CommandInput]
        public int Priority
        {
            get { return _priority ?? 1; }
            set { _priority = value; }
        }

        protected bool NameIsValid
        {
            get { return Regex.IsMatch(Name, @"^\S+$"); }
        }

        IConfigurationManager ConfigurationManager
        {
            get { return ServiceLocator.GetService<IConfigurationManager>(); }
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
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

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return ValidateName;
            yield return NameAlreadyExists;
        }

        int GetNewRemotePosition(RemoteRepositories repositories)
        {
            if (_priority.HasValue)
                return _priority.Value;
            return repositories.Count > 0 ? repositories.Values.Max(r => r.Priority) + 1 : 1;
        }

        IEnumerable<ICommandOutput> NameAlreadyExists()
        {
            if (ConfigurationManager.LoadRemoteRepositories().ContainsKey(Name))
                yield return new Error("A repository with the name '{0}' already exists.", Name);
        }

        IEnumerable<ICommandOutput> ValidateName()
        {
            if (!NameIsValid)
                yield return new Error("The 'Name' parameter is invalid.");
        }
    }
}