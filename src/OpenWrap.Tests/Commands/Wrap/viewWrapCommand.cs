using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;

namespace viewWrap_specs
{
    public class viewing_a_package : command_context<ViewWrapCommand>
    {
        public viewing_a_package()
        {
            given_project_package("one-ring", "1.0", string.Empty, "depends: sauron 2.0");
            given_project_package("sauron", "2.0");

            when_executing_command("one-ring");
        }
        [Test]
        public void matching_package_is_returned()
        {
            var s = Results.OfType<ViewWrapCommandOutput>()
                    .ShouldHaveCountOf(1)
                    .First().ToString();
            Console.WriteLine(s);
            s.ShouldContain("name: one-ring");
        }
        [Test]
        public void displays_dependencies()
        {
            Results.OfType<ViewWrapCommandOutput>()
                    .ShouldHaveCountOf(1)
                    .First().ToString().ShouldContain("dependencies: sauron 2.0");
        }

    }

    public class viewing_a_package_version : command_context<ViewWrapCommand>
    {
        public viewing_a_package_version()
        {
            given_project_package("one-ring", "0.1.0");
            given_project_package("one-ring", "0.1.1");
            given_project_package("sauron", "2.0");

            when_executing_command("one-ring", "-version", "0.1");
        }
        [Test]
        public void matching_package_is_returned()
        {
            Results.OfType<ViewWrapCommandOutput>()
                    .ShouldHaveCountOf(1)
                    .First().ToString().ShouldContain("version: 0.1.1");
        }
    }

    public class viewing_a_system_package : command_context<ViewWrapCommand>
    {
        public viewing_a_system_package()
        {
            given_project_package("one-ring", "1.0");
            given_system_package("one-ring", "1.0");
            given_system_package("one-ring", "2.0");

            when_executing_command("one-ring", "-system");
        }
        [Test]
        public void matching_package_is_returned()
        {
            Results.OfType<ViewWrapCommandOutput>()
                    .ShouldHaveCountOf(1)
                    .First().ToString().ShouldContain("version: 2.0");
        }
    }
}
