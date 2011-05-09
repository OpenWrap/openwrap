﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Services;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands.Remote.Add
{
    public class when_adding_a_remote_with_existing_name
         : command_context<AddRemoteCommand>
    {
        public when_adding_a_remote_with_existing_name()
        {
            given_remote_configuration(new RemoteRepositories{{"iron-hills", null}});
            when_executing_command("iron-hills http://lotr.org/iron-hills");
        }
        [Test]
        public void an_error_is_returned()
        {
            Results.FirstOrDefault(x => x.Success() == false)
                    .ShouldNotBeNull();
        }
    }
}