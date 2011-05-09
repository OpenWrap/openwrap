using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.set_wrap
{
    class set_wrap_conflicting_version_inputs : command_context<SetWrapCommand>
    {
        public set_wrap_conflicting_version_inputs()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -version 2.0.0 -maxversion 3.0.0");
        }

        [Test]
        public void error_returned()
        {
            Results.First().ShouldBeOfType<Error>();
        }
    }
}