using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.set_wrap
{
    class set_wrap_content_true : command_context<SetWrapCommand>
    {
        public set_wrap_content_true()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -content true");
        }

        [Test]
        public void dependency_content_is_true()
        {
            Environment.Descriptor.Dependencies.First().ContentOnly.ShouldBeTrue();
        }

        [Test]
        public void dependency_anchored_unchanged()
        {
            Environment.Descriptor.Dependencies.First().Anchored.ShouldBeFalse();
        }
    }
}
