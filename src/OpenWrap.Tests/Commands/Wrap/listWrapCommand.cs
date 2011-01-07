using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.context;

namespace listWrap_specs
{
    public class filtering_project_package_list_by_name : command_context<ListWrapCommand>
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
    public class filtering_project_with_different_casing : command_context<ListWrapCommand>
    {
        public filtering_project_with_different_casing()
        {
            given_project_package("one-ring", "1.0");
            given_dependency("depends: one-ring");

            when_executing_command("*Rin*");
        }
        [Test]
        public void casing_is_ignored()
        {
            Results.OfType<PackageFoundCommandOutput>()
                    .ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("one-ring");
        }
    }
    public class listing_packages_from_all_repositories : command_context<ListWrapCommand>
    {
        public listing_packages_from_all_repositories()
        {
            given_remote_repository("first");
            given_remote_repository("second");
            given_remote_package("first", "one-ring", "1.0.0");
            given_remote_package("second", "ring-of-power", "1.0.0");

            when_executing_command("ring", "-remote");
        }

        [Test]
        public void packages_are_found_in_any_remote()
        {
            Results.OfType<PackageFoundCommandOutput>()
                    .ShouldHaveCountOf(2)
                    .Check(x => x.ShouldHaveAtLeastOne(n => n.Name.Equals("one-ring")))
                    .Check(x => x.ShouldHaveAtLeastOne(n => n.Name.Equals("ring-of-power")));
        }
    }

    public class specifying_detailed_flag_when_listing_packages : command_context<ListWrapCommand>
    {
        public specifying_detailed_flag_when_listing_packages()
        {
            given_remote_repository("first");
            given_remote_detailed_package("first", "one-ring", "1.0.0", "This is one to rule them all");

            when_executing_command("ring", "-remote", "first", "-detailed");
        }

        [Test]
        public void description_is_output()
        {
            Results.OfType<PackageFoundCommandOutput>().ShouldHaveCountOf(1);
            ((PackageFoundCommandOutput)Results[0]).Message.Contains("This is one to rule them all").ShouldBe(true);
        }
    }

}
