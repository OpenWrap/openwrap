using System.Collections.Generic;
using System.Linq;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Remote
{
    public abstract class AbstractRemoteCommand : AbstractCommand
    {
        protected IConfigurationManager ConfigurationManager
        {
            get { return ServiceLocator.GetService<IConfigurationManager>(); }
        }

        protected IEnumerable<IRemoteRepositoryFactory> Factories { get { return ServiceLocator.GetService<IEnumerable<IRemoteRepositoryFactory>>(); } }

        protected int MoveRepositoriesToHigherPriority(int val, RemoteRepositories repositories)
        {
            int reorderFrom = val;
            int reorderTo = reorderFrom;
// ReSharper disable AccessToModifiedClosure
            while (repositories.Any(_ => _.Value.Priority == reorderTo))
// ReSharper restore AccessToModifiedClosure
                reorderTo++;

            foreach (var repo in repositories.Where(_ => _.Value.Priority >= reorderFrom && _.Value.Priority < reorderTo))
                repo.Value.Priority = repo.Value.Priority + 1;
            return val;
        }
    }
}