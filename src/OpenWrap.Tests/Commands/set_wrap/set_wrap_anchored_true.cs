using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;

namespace Tests.Commands.set_wrap
{
    class set_wrap_anchored_true : command_context<SetWrapCommand>
    {
        public set_wrap_anchored_true()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", "1.0.0.0");
            
            when_executing_command("sauron -anchored true");
        }

        [Test]
        public void dependency_anchored_is_true()
        {
            Environment.Descriptor.Dependencies.First().Anchored.ShouldBeTrue();
        }

        [Test]
        public void dependency_contentonly_unchanged()
        {
            Environment.Descriptor.Dependencies.First().ContentOnly.ShouldBeFalse();
        }
    }
}