using System;
using System.Collections.Generic;
using OpenWrap.Configuration;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "set")]
    public class SetRemoteCommand : AbstractRemoteCommand
    {
        int? _position;

        [CommandInput(Position = 0, IsRequired = true)]
        public string Name { get; set; }

        [CommandInput]
        public int Priority
        {
            get { return _position ?? 1; }
            set { _position = value; }
        }

        [CommandInput]
        public string NewName { get; set; }

        [CommandInput]
        public string Href { get; set; }

        IConfigurationManager ConfigurationManager { get { return Services.ServiceLocator.GetService<IConfigurationManager>(); } }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            var repositories = ConfigurationManager.Load<RemoteRepositories>();

            RemoteRepository remote;
            if (!repositories.TryGetValue(Name, out remote))
            {
                yield return new Error("Could not find remote repository named: " + Name);
                yield break;
            }

            HandlePrioritySetting(repositories, remote);

            if (!string.IsNullOrEmpty(NewName))
            {
                if (repositories.ContainsKey(NewName))
                {
                    yield return new Error("Repository with name '{0}' already present.");
                    yield break;
                }
                remote.Name = NewName;
            }

            if (Href != null)
            {
                var foundRepo = Factories.Select(x => x.FromUserInput(Href)).NotNull().FirstOrDefault();

                if (foundRepo == null) throw new InvalidOperationException();
                remote.FetchRepository.Token = foundRepo.Token;
                remote.PublishRepositories = foundRepo.Feature<ISupportPublishing>() != null
                                                     ? new List<RemoteRepositoryEndpoint> { new RemoteRepositoryEndpoint{Token=foundRepo.Token} }
                                                     : new List<RemoteRepositoryEndpoint>(0);
            }

            ConfigurationManager.Save(repositories);
        }

        void HandlePrioritySetting(RemoteRepositories repositories, RemoteRepository remote)
        {
            if (!_position.HasValue || remote.Priority == _position)
                return;
            
            var prioHasIncreased = remote.Priority > _position;
            
            var otherRepositories = repositories.Values.Except(remote.ToEnumerable());

            var vars = prioHasIncreased
                                        ? new
                                        {
                                            RelevantRepositories = otherRepositories.OrderBy(r => r.Priority).ToList(),
                                            SkipCondition = new Func<RemoteRepository, bool>(r => r.Priority < _position),
                                            PriorityMutator = new Func<int, int>(i => i + 1)
                                        }
                                        : new
                                        {
                                            RelevantRepositories = otherRepositories.OrderByDescending(r => r.Priority).ToList(),
                                            SkipCondition = new Func<RemoteRepository, bool>(r => r.Priority > _position),
                                            PriorityMutator = new Func<int, int>(i => i - 1)
                                        };

            if (!vars.RelevantRepositories.Any(r => r.Priority == _position))
                return;
            
            var lastPriority = (int)_position;

            foreach (var repository in vars.RelevantRepositories.SkipWhile(vars.SkipCondition))
            {
                if (repository.Priority == lastPriority)
                    repository.Priority = (lastPriority = vars.PriorityMutator(lastPriority));
                else
                    break;
            }

            remote.Priority = _position.Value;
        }

    }
}
