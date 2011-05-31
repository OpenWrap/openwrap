using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Collections;
using OpenWrap.Configuration;
using OpenWrap.Repositories;


namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "add")]
    public class AddRemoteCommand : AbstractRemoteCommand
    {
        int? _priority;

        [CommandInput(Position = 1)]
        public string Href { get; set; }

        [CommandInput(Position = 0, IsRequired = true)]
        public string Name { get; set; }

        [CommandInput]
        public int Priority
        {
            get { return _priority ?? 1; }
            set { _priority = value; }
        }

        [CommandInput]
        public string Publish { get; set; }

        protected bool NameIsValid
        {
            get { return Regex.IsMatch(Name, @"^\S+$"); }
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var repositories = ConfigurationManager.Load<RemoteRepositories>();

            return repositories.ContainsKey(Name)
                       ? Append(repositories)
                       : AddNew(repositories);
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return ValidateName;
            yield return NameAlreadyExists;
        }

        IEnumerable<ICommandOutput> AddNew(RemoteRepositories repositories)
        {
            var repositoryInput = Href??Name;
            var repository = Factories.Select(x => x.FromUserInput(repositoryInput)).NotNull().FirstOrDefault();
            if (repository == null)
            {
                yield return UnknownRepositoryType(repositoryInput);
                yield break;
            }
            var publishTokens = (repository.Feature<ISupportPublishing>() != null) ? new List<string> { repository.Token } : new List<string>();

            int position = GetNewRemotePriority(repositories);

            repositories[Name] = new RemoteRepository
            {
                FetchRepository = repository.Token,
                PublishRepositories = publishTokens,
                Name = Name,
                Priority = position
            };
            ConfigurationManager.Save(repositories);
            yield return new GenericMessage(string.Format("Remote repository '{0}' added.", Name));
        }

        Error UnknownRepositoryType(string repositoryInput)
        {
            return new Error("The address '{0}' was not recognized as a known repository type.", repositoryInput);
        }

        IEnumerable<ICommandOutput> Append(RemoteRepositories repositories)
        {
            var existingReg = repositories[Name];
            var publishRepo = Factories.Select(x => x.FromUserInput(Publish)).NotNull().FirstOrDefault();
            if (publishRepo == null)
            {
                yield return UnknownRepositoryType(Publish);
                yield break;
            }
            if (publishRepo.Feature<ISupportPublishing>() == null)
            {
                yield return new Error("The path '{0}' is not recognized as a repository that can be published to.");
                yield break;
            }
            existingReg.PublishRepositories.Add(publishRepo.Token);

            ConfigurationManager.Save(repositories);
            yield return new Info("Publish endpoint added to remote repository '{0}'.", Name);
        }

        int GetNewRemotePriority(RemoteRepositories repositories)
        {
            if (_priority.HasValue)
                return _priority.Value;
            return repositories.Count > 0 ? repositories.Values.Max(r => r.Priority) + 1 : 1;
        }

        IEnumerable<ICommandOutput> NameAlreadyExists()
        {
            var configs = ConfigurationManager.Load<RemoteRepositories>();
            if (configs.ContainsKey(Name) && Publish == null)
                yield return new Error("A repository with the name '{0}' already exists. Try specifying a different name.", Name);
        }

        IEnumerable<ICommandOutput> ValidateName()
        {
            if (!NameIsValid)
                yield return new Error("The 'Name' parameter is invalid. Identifiers ");
        }
    }
}