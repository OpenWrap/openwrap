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
        }

        [Test]
        public void depends_contains_both_vertices()
        {
            Environment.Descriptor.Dependencies.First().ToString().ShouldBe("sauron >= 2.0.0 and < 3.0.0");
        }
    }
}
