using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using OpenWrap.Commands;


namespace OpenWrap.Tests.Commands.Wrap
{
    public class cleaning_a_non_existing_wrap : command_context<CleanWrapCommand>
    {
        public cleaning_a_non_existing_wrap()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("lionel", "1.2.3.4");

            when_executing_command("richie", "-Project");
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

    public class cleaning_a_wrap_with_one_version : command_context<CleanWrapCommand>
    {
        public cleaning_a_wrap_with_one_version()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("lionel", "1.2.3.4");
            given_dependency("depends: lionel");
                    
            when_executing_command("lionel", "-Project");
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

    public class cleaning_a_wrap_with_two_versions : command_context<CleanWrapCommand>
    {
        static readonly string LionelVersion = "1.0.0.123";
        public cleaning_a_wrap_with_two_versions()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("lionel", "1.0.0.0");
            given_project_package("lionel", LionelVersion);
            given_dependency("depends: lionel");
            when_executing_command("lionel", "-Project");
        }

        [Test]
        public void only_the_latest_is_kept()
        {
            Environment.ProjectRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(1)
                .ShouldHaveAll(v => v.Version.ToString().Equals(LionelVersion));
        }
        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNo(x => x.Type == CommandResultType.Error);
        }
    }

    public class cleaning_package_from_system_repository : command_context<CleanWrapCommand>
    {
        static readonly string LionelVersion = "1.0.0.123";

        public cleaning_package_from_system_repository()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("lionel", "1.0.0.0");
            given_project_package("lionel", LionelVersion);


            given_system_package("lionel", "1.0.0.0");
            given_system_package("lionel", LionelVersion);

            when_executing_command("lionel", "-system");
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
        public void the_project_repository_should_remain_the_same()
        {
            Environment.ProjectRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(2);
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNoError();
        }
    }

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

            when_executing_command("lionel", "-sys", "-proj");
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
            Results.ShouldHaveNo(x => x is Error);
        }
    }

}
