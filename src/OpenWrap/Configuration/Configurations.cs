using System;

namespace OpenWrap.Configuration
{
    public static class ConfigurationExtensions
    {

        public static RemoteRepositories LoadRemoteRepositories(this IConfigurationManager configurationManager)
        {
            return configurationManager.Load<RemoteRepositories>();
        }

        public static void SaveRemoteRepositories(this IConfigurationManager configurationManager, RemoteRepositories repositories)
        {
            configurationManager.Save(repositories);
        }
   
    }
    public static class Configurations
    {
        public static class Addresses
        {
            public static readonly Uri BaseUri = new Uri("http://configuration.openwrap.org");
            public static readonly Uri RemoteRepositories = new Uri("http://configuration.openwrap.org/remote-repositories");
        }
    }
}