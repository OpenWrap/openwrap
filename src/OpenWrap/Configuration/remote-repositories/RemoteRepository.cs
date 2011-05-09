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

        public string FetchRepository { get; set; }
        public ICollection<string> PublishRepositories { get; set; }
        string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value.Trim(); }
        }

        public int Priority { get; set; }
    }
}