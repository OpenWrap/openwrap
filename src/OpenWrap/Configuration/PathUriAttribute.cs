using System;

namespace OpenWrap.Configuration
{
    public class PathUriAttribute : Attribute
    {
        public string Uri { get; private set; }

        public PathUriAttribute(string uri)
        {
            Uri = uri;
        }
    }
}