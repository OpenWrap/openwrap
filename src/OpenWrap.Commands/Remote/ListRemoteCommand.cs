using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Configuration;
using OpenWrap.Configuration.remote_repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    [Command(Noun="Remote", Verb="List")]
    public class ListRemoteCommand : AbstractCommand
    {
        IConfigurationManager ConfigurationManager { get { return Services.Services.GetService<IConfigurationManager>(); } }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return ConfigurationManager.LoadRemoteRepositories()
                    .Select(x => new RemoteRepositoryMessage(this, x.Key, x.Value))
                    .Cast<ICommandOutput>();
        }
    }

    public class RemoteRepositoryMessage : ICommandOutput
    {
        public string Name { get; set; }
        public RemoteRepository RemoteRepository { get; set; }

        public RemoteRepositoryMessage(ICommand sourceCommand, string name, RemoteRepository remoteRepository)
        {
            Name = name;
            RemoteRepository = remoteRepository;
            Source = sourceCommand;
        }
        public override string ToString()
        {
            return string.Format("{0,15}:{1}", Name, RemoteRepository.Href);
        }

        public bool Success
        {
            get { return true; }
        }

        public ICommand Source { get; private set; }

        public CommandResultType Type
        {
            get { return CommandResultType.Info; }
        }
    }
}
