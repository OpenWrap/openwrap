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
                            { "primus", new RemoteRepository { Name = "primus", Priority = 1 } },
                            { "secundus", new RemoteRepository { Name = "secundus", Priority = 2 } },
                            { "terz", new RemoteRepository { Name = "terz", Priority = 3 } }
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
            when_executing_command("secundus", "-priority", "1");
        }

        [Test]
        public void the_second_repository_has_new_priority()
        {
            var remote = TryGetRepository("secundus");
            remote.Priority.ShouldBe(1);
        }
    }

    public class when_changing_repository_name : set_remote
    {
        public when_changing_repository_name()
        {
            when_executing_command("secundus", "-newname", "vamu");
        }

        [Test]
        public void the_second_repository_has_new_name()
        {
            var remote = TryGetRepository("secundus");
            remote.Name.ShouldBe("vamu");
        }
    }

    public class when_changing_repository_name_to_existing : set_remote
    {
        public when_changing_repository_name_to_existing()
        {
            when_executing_command("secundus", "-newname", "primus");
        }

        [Test]
        public void should_return_error()
        {
            Results.ShouldContain<Error>();
        }
    }

    public class moving_repository_to_new_priority_case1 : set_remote
    {
        public moving_repository_to_new_priority_case1()
        {
            when_executing_command("terz", "-priority", "1");
        }

        [Test]
        public void rearranges_priorities()
        {
            TryGetRepository("terz").Priority.ShouldBe(1);
            TryGetRepository("primus").Priority.ShouldBe(2);
            TryGetRepository("secundus").Priority.ShouldBe(3);
        }
    }

    public class moving_repository_to_new_priority_case2 : set_remote
    {
        public moving_repository_to_new_priority_case2()
        {
            when_executing_command("secundus", "-priority", "1");
        }

        [Test]
        public void rearranges_priorities()
        {
            TryGetRepository("secundus").Priority.ShouldBe(1);
            TryGetRepository("primus").Priority.ShouldBe(2);
            TryGetRepository("terz").Priority.ShouldBe(3);
        }
    }

    public class moving_repository_to_new_priority_case3 : set_remote
    {
        public moving_repository_to_new_priority_case3()
        {
            when_executing_command("primus", "-priority", "2");
        }

        [Test]
        public void rearranges_priorities()
        {
            TryGetRepository("secundus").Priority.ShouldBe(1);
            TryGetRepository("primus").Priority.ShouldBe(2);
            TryGetRepository("terz").Priority.ShouldBe(3);
        }
    }

    public class moving_repository_to_new_priority_case4 : set_remote
    {
        public moving_repository_to_new_priority_case4()
        {
            when_executing_command("secundus", "-priority", "3");
        }

        [Test]
        public void rearranges_priorities()
        {
            TryGetRepository("primus").Priority.ShouldBe(1);
            TryGetRepository("terz").Priority.ShouldBe(2);
            TryGetRepository("secundus").Priority.ShouldBe(3);
        }
    }
}
