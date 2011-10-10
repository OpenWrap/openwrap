using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.clean_wrap.locks
{
    public class existing_lock : command<CleanWrapCommand>
    {
        public existing_lock()
        {
            given_project_package("nenya", "1.2.3.4");
            given_project_package("nenya", "1.2.3.5");
            given_locked_package("nenya", "1.2.3.4");

            given_dependency("depends: nenya");

            when_executing_command("nenya");
        }

        [Test]
        public void locked_package_is_not_cleaned()
        {
            Environment.ProjectRepository.ShouldHavePackage("nenya", "1.2.3.4");
        }

        [Test]
        public void unlocked_package_is_cleaned()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("nenya", "1.2.3.5");
            
        }
    }
}