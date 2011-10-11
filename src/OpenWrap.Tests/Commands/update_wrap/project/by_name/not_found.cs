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
    public class not_found : contexts.update_wrap
    {
        public not_found()
        {
            given_project_package("sauron", "1.0.0");
            given_dependency("depends: sauron");
            when_executing_command("sauron");
        }

        [Test]
        public void command_fails()
        {
            Results.ShouldHaveWarning();
        }
        [Test]
        public void not_found_error_is_displayed()
        {
            Results.ShouldHaveOne<PackageOnlyInCurrentRepository>()
                .PackageName.ShouldBe("sauron"); 
        }
    }
}