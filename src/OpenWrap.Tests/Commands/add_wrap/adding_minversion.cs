using NUnit.Framework;
using OpenWrap.Commands.contexts;

namespace OpenWrap.Tests.Commands
{
    class adding_minversion : add_wrap_command
    {
        public adding_minversion()
        {
            given_file_based_project_repository();
            given_system_package("sauron", "1.0.0");
            given_system_package("sauron", "2.0.0");
            when_executing_command("sauron", "-minversion", "1.0.0");
        }

        [Test]
        public void v2_package_added()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "2.0.0");
        }
    }
}