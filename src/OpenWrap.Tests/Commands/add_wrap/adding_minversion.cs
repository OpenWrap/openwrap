using NUnit.Framework;
using Tests.Commands.update_wrap;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.add_wrap
{
    class adding_minversion : contexts.add_wrap
    {
        public adding_minversion()
        {
            given_file_based_project_repository();
            given_system_package("sauron", "1.0.0");
            given_system_package("sauron", "2.0.0");
            when_executing_command("sauron -minversion 1.0.0");
        }

        [Test]
        public void v2_package_added()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "2.0.0");
        }
    }
}