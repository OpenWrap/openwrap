using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Configuration.remote_repositories;
using OpenWrap.Services;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands.Remote.Add
{
    public class when_adding_a_remote_with_existing_name
         : context.command_context<AddRemoteCommand>
    {
        public when_adding_a_remote_with_existing_name()
        {
            given_remote_configuration(new RemoteRepositories{{"iron-hills", null}});
            when_executing_command("iron-hills", "http://lotr.org/iron-hills");
        }
        [Test]
        public void an_error_is_returned()
        {
            Results.FirstOrDefault(x => x.Success() == false)
                    .ShouldNotBeNull();
        }
    }
    public class when_adding_a_new_remote : context.command_context<AddRemoteCommand>
    {
        public when_adding_a_new_remote()
        {
            given_remote_configuration(new RemoteRepositories());
            when_executing_command("iron-hills", "http://lotr.org/iron-hills");
        }
        [Test]
        public void remote_is_added_to_the_list()
        {
            Services.Services.GetService<IConfigurationManager>()
                    .LoadRemoteRepositories()
                    .ContainsKey("iron-hills")
                    .ShouldBeTrue();
        }
    }
}
