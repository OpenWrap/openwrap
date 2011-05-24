using System;
using System.Text;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Configuration;
using StreamExtensions = OpenWrap.IO.StreamExtensions;

namespace Tests.contexts
{

    public abstract class configuration<T> : OpenWrap.Testing.context
            where T:new()
    {
        protected T Entry;
        protected DefaultConfigurationManager DefaultConfigurationManager;
        protected InMemoryFileSystem FileSystem;
        protected IDirectory ConfigurationDirectory;

        public configuration()
        {
            FileSystem = new InMemoryFileSystem();

            ConfigurationDirectory = FileSystem.GetDirectory(@"c:\data\config").MustExist();
            DefaultConfigurationManager = new DefaultConfigurationManager(ConfigurationDirectory);
        }

        protected void when_loading_configuration(Uri configurationUri = null)
        {
            try
            {
                Entry = DefaultConfigurationManager.Load<T>(configurationUri);
            }catch(Exception error)
            {
                Error = error;
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