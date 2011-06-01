using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Services;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.contexts
{
    public class add_remote : command_context<AddRemoteCommand>
    {
        protected RemoteRepositories StoredRemotesConfig { get { return ServiceLocator.GetService<IConfigurationManager>().Load<RemoteRepositories>(); } }
        
    }
}