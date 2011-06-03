using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWrap.Collections;
using OpenWrap.Commands.Messages;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;


namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "add")]
    public class AddRemoteCommand : AbstractRemoteCommand
    {
        [CommandInput(Position = 1)]
        public string Href { get; set; }

        [CommandInput(Position = 0, IsRequired = true)]
        public string Name { get; set; }

        [CommandInput]
        public string Password { get; set; }

        [CommandInput]
        public int? Priority { get; set; }

        [CommandInput]
        public string Publish { get; set; }

        [CommandInput]
        public string Username { get; set; }

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
            yield return NameAvailable;
            yield return AuthIsCorrect;
        }

        IEnumerable<ICommandOutput> AddNew(RemoteRepositories repositories)
        {
            var repositoryInput = Href ?? Name;
            var repository = Factories.Select(x => x.FromUserInput(repositoryInput)).NotNull().FirstOrDefault();
            if (repository == null)
            {
                yield return new UnknownEndpointType(repositoryInput);
                yield break;
            }
            var publishTokens = (repository.Feature<ISupportPublishing>() != null)
                                    ? new List<RemoteRepositoryEndpoint>
                                    {
                                        new RemoteRepositoryEndpoint { Token = repository.Token, Username = Username, Password = Password }
                                    }
                                    : new List<RemoteRepositoryEndpoint>();

            int position = GetNewRemotePriority(repositories);

            repositories[Name] = new RemoteRepository
            {
                FetchRepository = { Token = repository.Token, Username = Username, Password = Password },
                PublishRepositories = publishTokens,
                Name = Name,
                Priority = position
            };
            ConfigurationManager.Save(repositories);
            yield return new RemoteAdded(Name);
        }

        IEnumerable<ICommandOutput> Append(RemoteRepositories repositories)
        {
            var existingReg = repositories[Name];
            var publishRepo = Factories.Select(x => x.FromUserInput(Publish)).NotNull().FirstOrDefault();
            if (publishRepo == null)
            {
                yield return new UnknownEndpointType(Publish);
                yield break;
            }
            if (publishRepo.Feature<ISupportPublishing>() == null)
            {
                yield return new RemoteEndpointReadOnly(Publish);
                yield break;
            }
            existingReg.PublishRepositories.Add(new RemoteRepositoryEndpoint
            {
                Token = publishRepo.Token,
                Username = Username,
                Password = Password
            });

            ConfigurationManager.Save(repositories);
            yield return new RemotePublishEndpointAdded(Name, Publish);
        }

        IEnumerable<ICommandOutput> AuthIsCorrect()
        {
            if ((Username != null || Password != null) && (Username == null || Password == null))
                yield return new IncompleteCredentials();
        }

        int GetNewRemotePriority(RemoteRepositories repositories)
        {
            return Priority.HasValue
                       ? MoveRepositoriesToHigherPriority(Priority.Value, repositories)
                       : (repositories.Count > 0 ? repositories.Values.Max(r => r.Priority) + 1 : 1);
        }

        IEnumerable<ICommandOutput> NameAvailable()
        {
            if (ConfigurationManager.Load<RemoteRepositories>().ContainsKey(Name) && Publish == null)
                yield return new RemoteNameInUse(Name);
        }

        IEnumerable<ICommandOutput> NameValid()
        {
            if (!Regex.IsMatch(Name, @"^\S+$"))
                yield return new RemoteNameInvalid(Name);
        }
    }
}