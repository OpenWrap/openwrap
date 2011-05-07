using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Configuration;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.IO;
using OpenWrap.Testing;
using StreamExtensions = OpenWrap.IO.StreamExtensions;

namespace OpenWrap.Tests.Configuration
{
    class when_reading_dictionaries_of_values : context.configuration_entry<RemoteRepositories>
    {
        static readonly Uri repositoryUri = new Uri("http://configuration.openwrap.org/repository");

        public when_reading_dictionaries_of_values()
        {
            given_configuration_text(Configurations.Addresses.RemoteRepositories, "[remoterepository openwrap]\r\nfetchrepository: http://wraps.openwrap.org\r\n[remoterepository]\r\nfetchrepository:http://default.openwrap.org");

            when_loading_configuration(Configurations.Addresses.RemoteRepositories);
        }
        [Test]
        public void the_name_in_the_section_is_the_key_in_the_dictionary()
        {
            Entry.ContainsKey("openwrap").ShouldBeTrue();
        }
        [Test]
        public void entries_in_dictionary_have_their_properties_set()
        {
            Entry["openwrap"].FetchRepository.ShouldBe("http://wraps.openwrap.org");
        }
        [Test]
        public void sections_with_no_name_have_an_empty_key()
        {
            Entry.ContainsKey(string.Empty).ShouldBeTrue();
        }
    }
    class when_two_dictionary_values_with_same_name_are_present : context.configuration_entry<RemoteRepositories>
    {
        public when_two_dictionary_values_with_same_name_are_present()
        {
            given_configuration_text(Configurations.Addresses.RemoteRepositories, "[remoterepository openwrap]\r\nhref:http://wraps.openwrap.org\r\n[remoterepository openwrap]\r\nhref:http://default.openwrap.org");

            when_loading_configuration(Configurations.Addresses.RemoteRepositories);
        }
        [Test]
        public void an_error_is_triggered()
        {
            Error.ShouldBeOfType<InvalidConfigurationException>();
        }
    }
    class when_writing_configuration_files_back: context.configuration_entry<RemoteRepositories>
    {
        public when_writing_configuration_files_back()
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
        void when_saving_configuration(Uri uri)
        {
            DefaultConfigurationManager.Save(uri, Entry);
            Entry = DefaultConfigurationManager.LoadRemoteRepositories();
        }

        void given_configuration_object(RemoteRepositories remoteRepositories)
        {
            Entry = remoteRepositories;

        }
    }
    class when_configuration_file_is_not_present_and_there_is_a_default : context.configuration_entry<RemoteRepositories>
    {
        public when_configuration_file_is_not_present_and_there_is_a_default()
        {
            when_loading_configuration(Configurations.Addresses.RemoteRepositories);
        }
        [Test]
        public void a_default_value_is_returned()
        {
            Entry.ShouldBeSameInstanceAs(RemoteRepositories.Default);
        }
    }
    namespace context
    {

        public abstract class configuration_entry<T> : OpenWrap.Testing.context
            where T:new()
        {
            protected T Entry;
            protected DefaultConfigurationManager DefaultConfigurationManager;
            protected InMemoryFileSystem FileSystem;
            protected IDirectory ConfigurationDirectory;

            public configuration_entry()
            {
                FileSystem = new InMemoryFileSystem();

                ConfigurationDirectory = FileSystem.GetDirectory(@"c:\data\config").MustExist();
                DefaultConfigurationManager = new DefaultConfigurationManager(ConfigurationDirectory);
            }

            protected void when_loading_configuration(Uri configurationUri)
            {
                try
                {
                    Entry = DefaultConfigurationManager.Load<T>(configurationUri);
                }catch(Exception error)
                {
                    this.Error = error;
                }
            }

            protected Exception Error { get; set; }

            protected void given_configuration_text(Uri configurationUri, string textValue)
            {
                // add file to virtual file system by getting relative URI 
                var relativeUri = Configurations.Addresses.BaseUri.MakeRelativeUri(configurationUri).ToString();
                var file = ConfigurationDirectory.GetFile(relativeUri);
                using (var fs = file.OpenWrite())
                    StreamExtensions.Write(fs, Encoding.UTF8.GetBytes(textValue));


            }
        }
    }
}
