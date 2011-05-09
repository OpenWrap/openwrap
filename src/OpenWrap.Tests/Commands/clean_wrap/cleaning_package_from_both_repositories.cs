using NUnit.Framework;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.clean_wrap
{
    public class cleaning_package_from_both_repositories : command_context<CleanWrapCommand>
    {
        static readonly string LionelVersion = "1.0.0.123";

        public cleaning_package_from_both_repositories()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("lionel", "1.0.0.0");
            given_project_package("lionel", LionelVersion);
            given_dependency("depends: lionel");


            given_system_package("lionel", "1.0.0.0");
            given_system_package("lionel", LionelVersion);

            when_executing_command("lionel -sys -proj");
        }

        [Test]
        public void the_package_is_cleaned_from_the_system_repository()
        {
            Environment.SystemRepository
                    .PackagesByName["lionel"]
                    .ShouldHaveCountOf(1)
                    .ShouldHaveAll(v => v.Version.ToString().Equals(LionelVersion));
        }

        [Test]
        public void the_package_is_cleaned_from_the_project_repository()
        {
            Environment.ProjectRepository
                    .PackagesByName["lionel"]
                    .ShouldHaveCountOf(1)
                    .ShouldHaveAll(v => v.Version.ToString().Equals(LionelVersion));
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNoError();
        }
    }
}