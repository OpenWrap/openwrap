using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Collections;
using OpenWrap.Configuration;
using OpenWrap.Repositories;
using OpenWrap.Services;


namespace OpenWrap.Commands.Remote
{
    public abstract class AbstractRemoteCommand : AbstractCommand
    {
        protected IConfigurationManager ConfigurationManager
        {
            get { return ServiceLocator.GetService<IConfigurationManager>(); }
        }

        protected IEnumerable<IRemoteRepositoryFactory> Factories { get { return ServiceLocator.GetService<IEnumerable<IRemoteRepositoryFactory>>(); } }
    }

    [Command(Noun = "remote", Verb = "add")]
    public class AddRemoteCommand : AbstractRemoteCommand
    {
        int? _priority;

        [CommandInput(Position = 1, IsRequired = true)]
        public string Href { get; set; }

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

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var repositories = ConfigurationManager.LoadRemoteRepositories();
            var repository = Factories.Select(x => x.FromUserInput(Href)).NotNull().FirstOrDefault();
            var publishTokens = (repository.Feature<ISupportPublishing>() != null) ? new List<string> { repository.Token } : new List<string>();

            int position = GetNewRemotePriority(repositories);

            repositories[Name] = new RemoteRepository
            {
                    FetchRepository = repository.Token,
                    PublishRepositories = publishTokens,
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

        int GetNewRemotePriority(RemoteRepositories repositories)
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