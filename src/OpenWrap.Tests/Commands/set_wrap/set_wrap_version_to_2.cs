using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Commands.set_wrap
{
    class set_wrap_version_to_2 : command_context<SetWrapCommand>
    {
        public set_wrap_version_to_2()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -version 2.0.0");
            verticies = Environment.Descriptor.Dependencies.First().VersionVertices.ToArray();
        }

        VersionVertex[] verticies;

        [Test]
        public void dependency_has_one_version_vertex()
        {
            verticies.ShouldHaveCountOf(1);
        }

        [Test]
        public void dependency_version_equals_2()
        {
            verticies.First().Version.Equals(new Version("2.0.0"));
        }
    }
}