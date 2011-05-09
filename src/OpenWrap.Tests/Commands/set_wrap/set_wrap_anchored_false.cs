using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.Testing;

namespace Tests.Commands.set_wrap
{
    class set_wrap_anchored_false : command_context<SetWrapCommand>
    {
        public set_wrap_anchored_false()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", "1.0.0.0");
            ((InMemoryPackage)Environment.ProjectRepository.PackagesByName["sauron"].First()).Anchored = true;

            when_executing_command("sauron -anchored false");
        }

        [Test]
        public void dependency_anchored_is_false()
        {
            Environment.Descriptor.Dependencies.First().Anchored.ShouldBeFalse();
        }
    }
}