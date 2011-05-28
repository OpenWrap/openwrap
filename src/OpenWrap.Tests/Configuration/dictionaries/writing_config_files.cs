using System;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Configuration;
using OpenWrap.IO;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.dictionaries
{
    internal class writing_config_files : configuration<RemoteRepositories>
    {
        public writing_config_files()
        {
            given_configuration_object(RemoteRepositories.Default);
            when_saving_configuration(Configurations.Addresses.RemoteRepositories);
        }

        [Test]
        public void values_are_persisted()
        {
            ConfigurationDirectory.FindFile(
                    Configurations.Addresses.BaseUri.MakeRelativeUri(Configurations.Addresses.RemoteRepositories).ToString())
                    .ShouldNotBeNull()
                    .OpenRead().ReadString(Encoding.UTF8)
                    .ShouldContain(@"[remoterepository openwrap]")
                    .ShouldContain("fetchrepository: " + RemoteRepositories.Default["openwrap"].FetchRepository);
        }

        void given_configuration_object(RemoteRepositories remoteRepositories)
        {
            Entry = remoteRepositories;
        }

        void when_saving_configuration(Uri uri)
        {
            DefaultConfigurationManager.Save(Entry, uri);
            Entry = DefaultConfigurationManager.LoadRemoteRepositories();
        }
    }
}