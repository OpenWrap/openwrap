using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Commands.set_wrap
{
    class set_wrap_minversion_2 : command_context<SetWrapCommand>
    {
        public set_wrap_minversion_2()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -minversion 2.0.0");
        }

        [Test]
        public void dependency_version_is_greaterthan_2()
        {
            var vertex = Environment.Descriptor.Dependencies.First().VersionVertices.First();
            var greater = vertex as GreaterThanOrEqualVersionVertex;
            greater.Version.ShouldBe(new Version("2.0.0"));
        }
    }
}