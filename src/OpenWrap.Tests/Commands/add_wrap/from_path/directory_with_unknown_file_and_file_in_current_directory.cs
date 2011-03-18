using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap.from_path
{
    class directory_with_unknown_file_and_file_in_current_directory : contexts.add_wrap
    {
        public directory_with_unknown_file_and_file_in_current_directory()
        {
            given_current_directory("c:\\rohan");
            given_project_repository();
            given_file_package("c:\\rohan", "sauron", "2.0.0");
            FileSystem.GetDirectory(@"c:\mordor").MustExist();

            when_executing_command("sauron", "-from", @"c:\mordor");
        }

        [Test]
        public void package_is_not_added()
        {
            
            Environment.ProjectRepository.PackagesByName["sauron"].Count().ShouldBe(0);            
        }

        [Test]
        public void error_is_recorded()
        {
            Results.ShouldHaveError();            
        }
    }
}§