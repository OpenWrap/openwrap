using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Collections;
using OpenWrap.Commands.Messages;
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

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var repositories = ConfigurationManager.Load<RemoteRepositories>();

            return repositories.ContainsKey(Name)
                       ? Append(repositories)
                       : AddNew(repositories);
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return NameValid;
            yield return NameNotInUseForNewRemote;
            yield return AuthIsCorrect;
        }

        IEnumerable<ICommandOutput> AuthIsCorrect()
        {
            if ((Username != null || Password != null) && (Username == null || Password == null))
                yield return new IncompleteCredentials();
        }

        [CommandInput]
        public string Password { get; set; }

        [CommandInput]
        public string Username { get; set; }

        IEnumerable<ICommandOutput> AddNew(RemoteRepositories repositories)
        {
            var repositoryInput = Href??Name;
            var repository = Factories.Select(x => x.FromUserInput(repositoryInput)).NotNull().FirstOrDefault();
            if (repository == null)
            {
                yield return UnknownRepositoryType(repositoryInput);
                yield break;
            }
            var publishTokens = (repository.Feature<ISupportPublishing>() != null) ? new List<RemoteRepositoryEndpoint> { new RemoteRepositoryEndpoint{Token=repository.Token} } : new List<RemoteRepositoryEndpoint>();

            int position = GetNewRemotePriority(repositories);

            repositories[Name] = new RemoteRepository
            {
                FetchRepository = {Token=repository.Token},
                PublishRepositories = publishTokens,
                Name = Name,
                Priority = position
            };
            ConfigurationManager.Save(repositories);
            yield return new GenericMessage(string.Format("Remote repository '{0}' added.", Name));
        }

        Error UnknownRepositoryType(string repositoryInput)
        {
            return new UnknownRepositoryType(repositoryInput);
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
            existingReg.PublishRepositories.Add(new RemoteRepositoryEndpoint{Token=publishRepo.Token});

            ConfigurationManager.Save(repositories);
            yield return new Info("Publish endpoint added to remote repository '{0}'.", Name);
        }

        int GetNewRemotePriority(RemoteRepositories repositories)
        {
            if (_priority.HasValue)
                return _priority.Value;
            return repositories.Count > 0 ? repositories.Values.Max(r => r.Priority) + 1 : 1;
        }

        IEnumerable<ICommandOutput> NameNotInUseForNewRemote()
        {
            if (ConfigurationManager.Load<RemoteRepositories>().ContainsKey(Name) && Publish == null)
                yield return new RemoteNameInUse(Name);
        }

        IEnumerable<ICommandOutput> NameValid()
        {
            if (!Regex.IsMatch(Name, @"^\S+$"))
                yield return new RemoteNameInvalid();
        }
    }

    class RemoteNameInvalid : Error
    {
        public RemoteNameInvalid() : base("The 'Name' parameter is invalid for a remote name. Identifiers cannot contain spaces.")
        {
        }
    }

    public class RemoteNameInUse : Error
    {
        public RemoteNameInUse(string name)
            :base("A repository with the name '{0}' already exists. Try specifying a different name.", name)
        {
            
        }
    }
}