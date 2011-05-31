using System.Collections.Generic;
using OpenWrap.Configuration;
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
    }
}