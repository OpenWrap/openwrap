using System.Collections.Generic;

namespace OpenWrap.Configuration.Remotes
{
    public class RemoteRepository
    {
        public RemoteRepository()
        {
            PublishRepositories = new List<RemoteRepositoryEndpoint>();
        }

        [Key("fetch")]
        public RemoteRepositoryEndpoint FetchRepository { get; set; }

        [Ignore]
        public string Name { get; set; }

        public int Priority { get; set; }

        [Key("publish")]
        public ICollection<RemoteRepositoryEndpoint> PublishRepositories { get; set; }
    }
}