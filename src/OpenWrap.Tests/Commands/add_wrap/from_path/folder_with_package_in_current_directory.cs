using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap.from_path
{
    class folder_with_package_in_current_directory : contexts.add_wrap
    {
        public folder_with_package_in_current_directory()
        {
            given_current_directory("c:\\rohan");
            given_project_repository();
            given_file_package("c:\\rohan", "sauron", "2.0.0");
            given_file_package("c:\\mordor", "sauron", "1.0.0");

            when_executing_command(@"sauron -from c:\mordor -version 1.0");
        }
        [Test]
        public void package_is_added()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].Count().ShouldBe(1);
        }

        [Test]
        public void package_is_from_directory_specified_in_from_parameter()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].Single().Version.ShouldBe("1.0.0");
        }
    }
}