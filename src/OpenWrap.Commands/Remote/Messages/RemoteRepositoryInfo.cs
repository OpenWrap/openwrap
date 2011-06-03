using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;

namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteRepositoryInfo : Info
    {
        public string Name { get; set; }
        public RemoteRepository RemoteRepository { get; set; }

        public RemoteRepositoryInfo(string name, RemoteRepository remoteRepository)
            : base("{0}. {1,15}: {2}", remoteRepository.Priority, name, remoteRepository.FetchRepository)
        {
            Name = name;
            RemoteRepository = remoteRepository;
        }
    }
}