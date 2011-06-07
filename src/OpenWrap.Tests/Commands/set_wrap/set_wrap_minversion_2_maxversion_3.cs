using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.set_wrap
{
    class set_wrap_minversion_2_maxversion_3 : command<SetWrapCommand>
    {
        public set_wrap_minversion_2_maxversion_3()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -minversion 2.0.0 -maxversion 3.0.0");

            vertices = Environment.Descriptor.Dependencies.First().VersionVertices.ToArray();
        }

        readonly VersionVertex[] vertices;

        [Test]
        public void vertex_0_is_greaterthanorequal_2()
        {
            var greater = vertices[0] as GreaterThanOrEqualVersionVertex;
            greater.Version.ShouldBe(new Version("2.0.0"));
        }

        [Test]
        public void vertex_1_is_lessthan_3()
        {
            var lessThan = vertices[1] as LessThanVersionVertex;
            lessThan.Version.ShouldBe(new Version("3.0.0"));
        }
    }
}