using System;
using System.Collections.Generic;
using System.Threading;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.AllPackages
{
    public class ReadPackagesForRepositories : CommandBase
    {
        public override void Execute(object parameter)
        {
            // todo: constructor-inject services
            IEnvironment environment = GetEnvironment();

            ReadPackagesFromRepositories(environment.RemoteRepositories);
        }

        private static void ReadPackagesFromRepositories(IEnumerable<IPackageRepository> repositories)
        {
            // Poll repositories (some of which will be remote) in parallel. 
            foreach (IPackageRepository repository in repositories)
            {
                IPackageRepository repositoryItem = repository;
                ReadPackagesForRepository command = new ReadPackagesForRepository();
                ThreadPool.QueueUserWorkItem(o => command.Execute(repositoryItem));
            }
        }

        private static IEnvironment GetEnvironment()
        {
            var environment = new CurrentDirectoryEnvironment();
            environment.Initialize();
            return environment;
        }
    }
}
