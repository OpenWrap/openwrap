using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.set_wrap
{
    class set_wrap_version_to_2 : command<SetWrapCommand>
    {
        public set_wrap_version_to_2()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", "1.0.0.0");

            when_executing_command("sauron -version 2.0");
        }

        [Test]
        public void dependency_has_exact_version()
        {
            Environment.Descriptor.Dependencies.First().ToString().ShouldBe("sauron = 2.0");

        }

        
    }
}
