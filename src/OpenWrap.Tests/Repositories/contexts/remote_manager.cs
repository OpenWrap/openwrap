using System.Collections.Generic;
using System.Linq;
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
        }
    }
}