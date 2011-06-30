using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.set_wrap
{
    class set_wrap_version_to_any : command<SetWrapCommand>
    {
        public set_wrap_version_to_any()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -anyversion");
        }

        [Test]
        public void dependency_version_is_any()
        {
            var vertices = Environment.Descriptor.Dependencies.First().VersionVertices;
            vertices.First().ShouldBeOfType<AnyVersionVertex>();
        }
    }
}