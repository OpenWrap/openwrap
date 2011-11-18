using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.update_wrap.project
{
    public class from_remote : contexts.update_wrap
    {
        DateTimeOffset? descriptor_modified_time;

        public from_remote()
        {
            given_dependency("depends: goldberry >= 2.0");

            given_project_package("goldberry", "2.0.0");

            given_system_package("goldberry", "2.1.0");
            given_remote_package("goldberry", "2.2.0".ToVersion());

            descriptor_modified_time = Environment.DescriptorFile.LastModifiedTimeUtc;
            when_executing_command();
        }

        [Test]
        public void package_is_not_installed_in_system_repo()
        {
            Environment.SystemRepository.PackagesByName["goldberry"].Last().Version.ShouldBe(new Version(2, 1, 0));
        }

        [Test]
        public void package_is_installed_in_project_repo()
        {
            Environment.ProjectRepository.PackagesByName["goldberry"].Last().Version.ShouldBe(new Version(2, 2, 0));
        }

        [Test]
        public void descriptor_file_is_touched()
        {
            descriptor_modified_time.Value.ShouldBeLessThan(Environment.DescriptorFile.LastModifiedTimeUtc.Value);
        }
    }
}