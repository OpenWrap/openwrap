using System;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.context;
using OpenWrap.Tests.Commands.Remote.Set.context;

namespace OpenWrap.Tests.Commands.Remote.Set
{
    namespace context
    {
        public class set_remote : command_context<SetRemoteCommand>
        {
            public set_remote()
            {
                given_remote_configuration(
                           new RemoteRepositories
                    {
                            { "paralox", new RemoteRepository { Name = "paralox", Priority = 1 } },
                            { "es", new RemoteRepository { Name = "es", Priority = 2 } }
                    });
            }

            public RemoteRepository TryGetRepository(string name)
            {
                var repositories = Services.Services.GetService<IConfigurationManager>().LoadRemoteRepositories();
                RemoteRepository rep;
                repositories.TryGetValue(name, out rep);
                return rep;
            }
        }
    }

    public class when_changing_remote_priority : set_remote
    {
        public when_changing_remote_priority()
        {
            when_executing_command("es", "-priority", "1");
        }

        [Test]
        public void the_second_repository_has_new_priority()
        {
            var remote = TryGetRepository("es");
            remote.Priority.ShouldBe(1);
        }
    }

    public class when_changing_repository_name : set_remote
    {
        public when_changing_repository_name()
        {
            when_executing_command("es", "-newname", "vamu");
        }

        [Test]
        public void the_second_repository_has_new_name()
        {
            var remote = TryGetRepository("es");
            remote.Name.ShouldBe("vamu");
        }
    }

    public class when_changing_repository_name_to_existing : set_remote
    {
        public when_changing_repository_name_to_existing()
        {
            when_executing_command("es", "-newname", "paralox");
        }

        [Test]
        public void should_return_error()
        {
            Results.ShouldContain<Error>();
        }
    }
    
}
