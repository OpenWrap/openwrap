using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Services;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.contexts
{
    public class add_remote : remote_command<AddRemoteCommand>
    {
    }
}