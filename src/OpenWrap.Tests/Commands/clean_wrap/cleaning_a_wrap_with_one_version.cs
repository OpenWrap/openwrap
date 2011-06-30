using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.clean_wrap
{
    public class cleaning_a_wrap_with_one_version : command<CleanWrapCommand>
    {
        public cleaning_a_wrap_with_one_version()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("lionel", "1.2.3.4");
            given_dependency("depends: lionel");
                    
            when_executing_command("lionel -Project");
        }

        [Test]
        public void maintains_the_existing_version()
        {
            Environment.ProjectRepository
                    .PackagesByName["lionel"]
                    .ShouldHaveCountOf(1);
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNoError();
        }
    }
}