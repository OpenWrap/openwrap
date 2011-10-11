using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageManagement;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.update_wrap.project.by_name
{
    public class already_up_to_date : contexts.update_wrap
    {
        public already_up_to_date()
        {
            given_project_package("sauron", "1.0.0");
            given_remote_package("sauron", "1.0.0".ToVersion());
            given_dependency("depends: sauron");
            when_executing_command("sauron");
        }

        [Test]
        public void command_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void up_to_date_is_displayed()
        {
            Results.ShouldHaveOne<PackageUpToDateResult>()
                .Check(_ => _.Package.Name.ShouldBe("sauron"))
                .Check(_ => _.DestinationRepository.ShouldBe(Environment.ProjectRepository));
        }
    }
}