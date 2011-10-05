using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests.Commands.update_wrap.project;

namespace Tests.Commands.add_wrap
{
    public class adding_doesnt_impact_unrelated_packages : contexts.add_wrap
    {
        public adding_doesnt_impact_unrelated_packages()
        {
            given_file_based_project_repository();

            given_project_package("one-ring", "1.0.0");
            given_dependency("depends: one-ring = 1.0");

            given_remote_package("sauron", "1.0.0".ToVersion());
            given_remote_package("one-ring", "1.0.1".ToVersion());
            when_executing_command("sauron");
        }

        [Test]
        public void unrelated_dependency_is_not_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("one-ring","1.0.0");
        }
    }
}