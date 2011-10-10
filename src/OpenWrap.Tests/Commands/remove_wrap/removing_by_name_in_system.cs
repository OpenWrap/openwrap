using System;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.remove_wrap
{
    public class removing_by_name_in_system : contexts.remove_wrap
    {
        public removing_by_name_in_system()
        {
            given_system_package("gandalf", "1.0.0.0");
            given_system_package("gandalf", "1.0.1.0");
            given_system_package("saruman", "99");
            given_project_package("gandalf", "1.0.0.0");
            when_executing_command("gandalf -system");
        }
        [Test]
        public void package_is_removed_from_system()
        {
            Environment.SystemRepository.PackagesByName["gandalf"]
                    .ShouldBeEmpty();
        }
        [Test]
        public void package_with_different_name_is_not_removed_from_system()
        {
            Environment.SystemRepository.PackagesByName["saruman"]
                    .ShouldHaveCountOf(1);
        }
        [Test]
        public void project_repository_is_unaffected()
        {
            Environment.ProjectRepository.ShouldHavePackage("gandalf", "1.0.0.0");
        }
    }
}