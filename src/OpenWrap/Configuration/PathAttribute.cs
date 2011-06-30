using System;

namespace OpenWrap.Configuration
{
    public class PathAttribute : Attribute
    {
        public PathAttribute(string uri)
        {
            Uri = uri;
        }

        public string Uri { get; private set; }
    }
}