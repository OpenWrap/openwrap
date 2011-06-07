using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace listWrap_specs
{
    public class filtering_project_package_list_by_name : command<ListWrapCommand>
    {
        public filtering_project_package_list_by_name()
        {
            given_project_package("one-ring", "1.0");
            given_project_package("sauron", "2.0");
            given_dependency("depends: one-ring");
            given_dependency("depends: sauron");

            when_executing_command("one*");
        }
        [Test]
        public void matching_package_is_returned()
        {
            Results.OfType<PackageFoundCommandOutput>()
                    .ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("one-ring");
        }
    }
}
