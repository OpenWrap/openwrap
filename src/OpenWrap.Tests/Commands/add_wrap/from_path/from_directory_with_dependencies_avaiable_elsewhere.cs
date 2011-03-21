using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap.from_path
{
    class from_directory_with_dependencies_avaiable_elsewhere : contexts.add_wrap
    {
        public from_directory_with_dependencies_avaiable_elsewhere()
        {
            given_current_directory(@"c:\\rohan");
            given_project_repository();
            given_remote_package("one-ring", "2.0.0".ToVersion());
            given_file_package(@"c:\mordor", "sauron", "1.0.0", "depends: one-ring");

            when_executing_command("sauron", "-from", @"c:\mordor");
        }

        [Test]
        public void package_is_added()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);

        }

        [Test]
        public void dependency_is_added()
        {
            package_is_in_repository(Environment.ProjectRepository, "one-ring", "2.0.0".ToVersion());
        }
    }
}