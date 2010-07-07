using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.IO;
using OpenWrap.IO.FileSystem.InMemory;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Configuration
{
    class when_reading_collection_of_values : context.configuration_entry<RemoteRepositories>
    {
        static readonly Uri repositoryUri = new Uri("http://configuration.openwrap.org/repository");

        public when_reading_collection_of_values()
        {
            given_configuration_text(ConfigurationEntries.RemoteRepositories, "[remoterepository openwrap]\r\nhref=http://wraps.openwrap.org");

            when_loading_configuration(ConfigurationEntries.RemoteRepositories);
        }
        [Test]
        public void the_property_value_is_assigned()
        {
            Entry.ContainsKey("openwrap").ShouldBeTrue();
            Entry["openwrap"].Href.ShouldBe("http://wraps.openwrap.org");
        }

    }
    public class RemoteRepositories : Dictionary<string, RemoteRepository>
    {
        
    }

    public class RemoteRepository
    {
        public string Name { get; set; }
        public Uri Href { get; set; }
    }

    namespace context
    {

        public class configuration_entry<T> : Testing.context
            where T:new()
        {
            protected T Entry;
            protected ConfigurationManager ConfigurationManager;
            protected InMemoryFileSystem FileSystem;
            protected IDirectory ConfigurationDirectory;

            public configuration_entry()
            {
                FileSystem = new InMemoryFileSystem();

                ConfigurationDirectory = FileSystem.GetDirectory(@"c:\data\config").EnsureExists();
                ConfigurationManager = new ConfigurationManager(ConfigurationDirectory);
            }

            protected void when_loading_configuration(Uri configurationUri)
            {
                Entry = ConfigurationManager.Load<T>(configurationUri);
            }

            protected void given_configuration_text(Uri configurationUri, string textValue)
            {
                // add file to virtual file system by getting relative URI 
                var relativeUri = ConfigurationEntries.BaseUri.MakeRelativeUri(configurationUri).ToString();
                var file = ConfigurationDirectory.GetFile(relativeUri);
                using (var fs = file.OpenWrite())
                    fs.Write(Encoding.UTF8.GetBytes(textValue));


            }
        }
    }
}
