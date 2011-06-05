using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes.Legacy;
using OpenWrap.Repositories;
using OpenWrap.Services;
using Tests.Commands.contexts;
using Tests.Repositories.manager;

namespace Tests.Repositories.contexts
{
    public abstract class remote_manager : command{
        protected List<IPackageRepository> FetchRepositories;
        protected List<IPackageRepository> PublishRepositories;
        protected IRemoteManager RemoteManager { get { return ServiceLocator.GetService<IRemoteManager>(); } }

        protected void when_listing_repositories(string name = null)
        {
            FetchRepositories = RemoteManager.FetchRepositories(name).ToList();
            PublishRepositories = RemoteManager.PublishRepositories(name).SelectMany(x=>x).ToList();
            ReloadRepositories();
        }

        protected void given_configuration_file(Uri configurationUri, string textValue)
        {
            given_configuration_file(configurationUri.ToString(), textValue);
        }
        protected void given_configuration_file(string configurationUri, string textValue)
        {
            // add file to virtual file system by getting relative URI 
            var relativeUri = ConstantUris.Base.MakeRelativeUri(ConstantUris.Base.Combine(configurationUri)).ToString();
            var file = Environment.ConfigurationDirectory.GetFile(relativeUri);
            using (var fs = file.OpenWrite())
                fs.Write(Encoding.UTF8.GetBytes(textValue));


        }

        protected void given_configuration(RemoteRepositories remoteRepositories)
        {
            ConfigurationManager.Save(remoteRepositories);
        }
    }
}