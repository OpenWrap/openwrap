using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.add_wrap.from_path
{
    class directory : contexts.add_wrap
    {
        public directory()
        {
            given_current_directory("c:\\rohan");
            given_project_repository();
            given_file_package("c:\\mordor", "sauron", "1.0.0");

            when_executing_command(@"sauron -from c:\mordor");
        }

        [Test]
        public void package_is_added()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].Count().ShouldBe(1);
        }
    }
}
