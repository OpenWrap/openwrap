using System.Collections.Generic;
using OpenWrap.Configuration;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun = "remote", Verb = "set")]
    public class SetRemoteCommand : AbstractCommand
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

        IConfigurationManager ConfigurationManager { get { return Services.Services.GetService<IConfigurationManager>(); } }

        public override IEnumerable<ICommandOutput> Execute()
        {
            var repositories = ConfigurationManager.LoadRemoteRepositories();
            RemoteRepository remote;
            if (!repositories.TryGetValue(Name, out remote))
            {
                yield return new Error("Could not find remote repository named: " + Name);
                yield break;
            }

            if (_position.HasValue)
            {
                remote.Priority = _position.Value;
            }

            if (!string.IsNullOrEmpty(NewName))
            {
                if (repositories.ContainsKey(NewName))
                {
                    yield return new Error("Repository with name '{0}' already present.");
                    yield break;
                }
                remote.Name = NewName;
            }

            ConfigurationManager.SaveRemoteRepositories(repositories);
        }
    }
}
