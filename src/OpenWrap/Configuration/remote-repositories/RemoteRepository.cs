using System;
using System.Collections.Generic;

namespace OpenWrap.Configuration
{
    public class RemoteRepository
    {
        public RemoteRepository()
        {
            PublishRepositories = new List<string>();
        }

        [Key("fetch")]
        public string FetchRepository { get; set; }

        [Key("publish")]
        public ICollection<string> PublishRepositories { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; }
    }
}