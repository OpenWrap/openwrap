using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.clean_wrap
{
    class cleaning_across_two_scopes : command<CleanWrapCommand>
    {
        public cleaning_across_two_scopes()
        {
            given_project_repository();
            given_dependency("depends: one-ring = 2.0");
            given_dependency("edge", "depends: one-ring = 3.0");
            given_project_package("one-ring", "2.0");
            given_project_package("one-ring", "3.0");

            when_executing_command();
        }

        [Test]
        public void both_versions_in_use_are_kept()
        {
            Environment.ProjectRepository.PackagesByName["one-ring"].Count().ShouldBe(2);

        }
    }
}
