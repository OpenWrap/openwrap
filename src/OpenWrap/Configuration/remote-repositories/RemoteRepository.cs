using System;

namespace OpenWrap.Configuration
{
    public class RemoteRepository
    {
        public Uri Href { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
    }
}