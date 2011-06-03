using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.Commands.Messages;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "set")]
    public class SetRemoteCommand : AbstractRemoteCommand
    {
        // TODO: Add a -fetch
        IPackageRepository _fetchRepo;
        IPackageRepository _publishRepo;
        RemoteRepositories _remotes;
        RemoteRepository _targetRemote;

        [CommandInput]
        public string Href { get; set; }

        [CommandInput(Position = 0, IsRequired = true)]
        public string Name { get; set; }

        [CommandInput]
        public string NewName { get; set; }

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
            HandlePrioritySetting(_remotes, _targetRemote);

            if (NewName != null)
                _targetRemote.Name = NewName;

            if (_fetchRepo != null)
            {
                _targetRemote.FetchRepository.Token = _fetchRepo.Token;
                _targetRemote.FetchRepository.Username = Username;
                _targetRemote.FetchRepository.Password = Password;

                _targetRemote.PublishRepositories = ToPublishEndpoints(_publishRepo ?? _fetchRepo);
            }
            else if (_publishRepo != null)
            {
                _targetRemote.PublishRepositories = ToPublishEndpoints(_publishRepo);
            }
            else if (_publishRepo == null && _fetchRepo == null && Username != null)
            {
                foreach (var config in _targetRemote.PublishRepositories.Concat(_targetRemote.FetchRepository))
                {
                    config.Username = Username;
                    config.Password = Password;
                }
            }

            ConfigurationManager.Save(_remotes);
            yield return new RemoteUpdated(Name);
        }

        protected override IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield return RemoteExists;
            yield return NewNameAvailable;
            yield return FetchRepoFound;
            yield return PublishRepoFound;
            yield return CredentialsComplete;
        }

        IEnumerable<ICommandOutput> CredentialsComplete()
        {
            if ((Username != null || Password != null) && (Username == null || Password == null))
                yield return new IncompleteCredentials();
        }

        IEnumerable<ICommandOutput> FetchRepoFound()
        {
            if (Href != null)
            {
                _fetchRepo = Factories.Select(x => x.FromUserInput(Href)).NotNull().FirstOrDefault();
                if (_fetchRepo == null)
                    yield return new UnknownEndpointType(Href);
            }
        }

        void HandlePrioritySetting(RemoteRepositories repositories, RemoteRepository remote)
        {
            if (!Priority.HasValue || remote.Priority == Priority)
                return;

            MoveRepositoriesToHigherPriority(Priority.Value, repositories);

            remote.Priority = Priority.Value;
        }

        IEnumerable<ICommandOutput> NewNameAvailable()
        {
            if (NewName != null && ConfigurationManager.Load<RemoteRepositories>().ContainsKey(NewName))
                yield return new RemoteNameInUse(NewName);
        }

        IEnumerable<ICommandOutput> PublishRepoFound()
        {
            if (Publish != null)
            {
                var publishRepo = Factories.Select(x => x.FromUserInput(Publish)).NotNull().FirstOrDefault();
                if (publishRepo == null)
                    yield return new UnknownEndpointType(Publish);
                else if (publishRepo.Feature<ISupportPublishing>() == null)
                    yield return new RemoteEndpointReadOnly(Publish);
                else
                    _publishRepo = publishRepo;
            }
        }

        IEnumerable<ICommandOutput> RemoteExists()
        {
            _remotes = ConfigurationManager.Load<RemoteRepositories>();
            if (_remotes.ContainsKey(Name) == false)
                yield return new UnknownRemoteName(Name);
            else
                _targetRemote = _remotes[Name];
        }

        ICollection<RemoteRepositoryEndpoint> ToPublishEndpoints(IPackageRepository repo)
        {
            if (repo == null || repo.Feature<ISupportPublishing>() == null) return new List<RemoteRepositoryEndpoint>(0);
            return new List<RemoteRepositoryEndpoint>
            {
                new RemoteRepositoryEndpoint
                {
                    Token = repo.Token,
                    Username = Username,
                    Password = Password
                }
            };
        }
    }
}