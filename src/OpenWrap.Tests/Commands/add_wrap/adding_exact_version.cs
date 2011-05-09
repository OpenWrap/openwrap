using NUnit.Framework;
using Tests.Commands.update_wrap;

namespace Tests.Commands.add_wrap
{
    class adding_exact_version : contexts.add_wrap
    {
        public adding_exact_version()
        {
            given_file_based_project_repository();
            given_system_package("sauron", "1.0.0");
            given_system_package("sauron", "2.0.0");
            when_executing_command("sauron -version 1.0.0");
        }

        [Test]
        public void v1_package_added()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "1.0.0");
        }
    }
}