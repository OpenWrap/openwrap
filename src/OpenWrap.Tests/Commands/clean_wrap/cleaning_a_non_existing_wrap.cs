using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.clean_wrap
{
    public class cleaning_a_non_existing_wrap : command_context<CleanWrapCommand>
    {
        public cleaning_a_non_existing_wrap()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("lionel", "1.2.3.4");

            when_executing_command("richie -Project");
        }

        [Test]
        public void maintains_all_other_wraps()
        {
            Environment.ProjectRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(1);
        }

        [Test]
        public void the_non_existence_is_reported()
        {
            Results.ShouldHaveError();
        }

    }
}
