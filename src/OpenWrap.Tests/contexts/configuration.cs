using System;
using System.Text;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenRasta.Client;
using OpenWrap.Configuration;
using Tests.Configuration;
using Tests.Configuration.dictionaries;


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

        protected void when_loading_configuration(string configurationUri = null)
        {
            try
            {
                Entry = DefaultConfigurationManager.Load<T>(configurationUri == null ? null : ConstantUris.Base.Combine(configurationUri));
            }catch(Exception error)
            {
                Error = error;
            }
        }

        protected Exception Error { get; set; }

        protected void given_configuration_text(Uri configurationUri, string textValue)
        {
            given_configuration_text(configurationUri.ToString(), textValue);
        }
        protected void given_configuration_text(string configurationUri, string textValue)
        {
            // add file to virtual file system by getting relative URI 
            var relativeUri = ConstantUris.Base.MakeRelativeUri(ConstantUris.Base.Combine(configurationUri)).ToString();
            var file = ConfigurationDirectory.GetFile(relativeUri);
            using (var fs = file.OpenWrite())
                fs.Write(Encoding.UTF8.GetBytes(textValue));


        }

        protected void when_saving_configuration(string path)
        {
            var pathUri = ConstantUris.URI_BASE.ToUri().Combine(path);
            DefaultConfigurationManager.Save(Entry, pathUri);
            Entry = DefaultConfigurationManager.Load<T>(pathUri);
        }

        protected void given_configuration(T config)
        {
            Entry = config;
        }
    }
}