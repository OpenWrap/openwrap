using NUnit.Framework;
using OpenWrap.Commands.contexts;
using Tests.Commands.contexts;

namespace OpenWrap.Tests.Commands
{
    class adding_exact_version : add_wrap
    {
        public adding_exact_version()
        {
            given_file_based_project_repository();
            given_system_package("sauron", "1.0.0");
            given_system_package("sauron", "2.0.0");
            when_executing_command("sauron", "-version", "1.0.0");
        }

        [Test]
        public void v1_package_added()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "1.0.0");
        }
    }
}