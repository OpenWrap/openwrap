using System;

namespace OpenWrap.Configuration
{
    public static class ConstantUris
    {
        public const string URI_BASE = "http://configuration.openwrap.org";
        public const string URI_REMOTES = "http://configuration.openwrap.org/remote-repositories";
        public static readonly Uri Base = new Uri(URI_BASE);
    }
}