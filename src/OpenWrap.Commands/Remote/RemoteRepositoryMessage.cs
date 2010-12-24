using OpenWrap.Configuration;

namespace OpenWrap.Commands.Remote
{
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
            return string.Format("{0}. {1,15}: {2}", RemoteRepository.Priority, Name, RemoteRepository.Href);
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