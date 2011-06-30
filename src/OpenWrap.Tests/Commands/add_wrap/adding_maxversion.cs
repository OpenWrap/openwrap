using NUnit.Framework;
using Tests.Commands.update_wrap;

namespace Tests.Commands.add_wrap
{
    class adding_maxversion : contexts.add_wrap
    {
        public adding_maxversion()
        {
            given_file_based_project_repository();
            given_system_package("sauron", "1.0.0");
            given_system_package("sauron", "2.0.0");
            when_executing_command("sauron -maxversion 2.0.0");
        }

        [Test]
        public void v1_package_added()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "1.0.0");
        }
    }
}