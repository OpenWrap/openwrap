using System;
using OpenWrap.Configuration.remote_repositories;

namespace OpenWrap.Configuration
{
    public static class Configurations
    {
        public static class Addresses
        {
            public static readonly Uri BaseUri = new Uri("http://configuration.openwrap.org");
            public static readonly Uri RemoteRepositories = new Uri("http://configuration.openwrap.org/remote-repositories");
        }
        public static RemoteRepositories RemoteRepositories(this IConfigurationManager configurationManager)
        {
            return configurationManager.Load<RemoteRepositories>(Addresses.RemoteRepositories);
        }
    }
    
}