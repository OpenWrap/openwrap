using System;

namespace OpenWrap.Configuration
{
    public class PathAttribute : Attribute
    {
        public string Uri { get; private set; }

        public PathAttribute(string uri)
        {
            Uri = uri;
        }
    }
}