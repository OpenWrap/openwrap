using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using OpenWrap.Commands;


namespace OpenWrap.Tests.Commands.Wrap
{
    public class cleaning_a_non_existing_wrap : context.command_context<CleanWrapCommand>
    {
        public cleaning_a_non_existing_wrap()
        {
            given_project_repository();
            given_project_package("lionel", new Version(1, 2, 3, 4));

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
            Results.ShouldHaveAtLeastOne(x => x is Error);
        }

    }

    public class cleaning_a_wrap_with_one_version : context.command_context<CleanWrapCommand>
    {
        public cleaning_a_wrap_with_one_version()
        {
            given_project_repository();
            given_project_package("lionel", new Version(1, 2, 3, 4));

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
        public void reports_success()
        {
            Results.ShouldHaveAtLeastOne(x => x is Success);
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNo(x => x is Error);
        }
    }

    public class cleaning_a_wrap_with_two_versions : context.command_context<CleanWrapCommand>
    {
        static readonly Version LionelVersion = new Version(1, 0, 0, 123);
        public cleaning_a_wrap_with_two_versions()
        {
            given_project_repository();
            given_project_package("lionel", new Version(1, 0, 0, 0));
            given_project_package("lionel", LionelVersion);

            when_executing_command("lionel", "-Project");
        }

        [Test]
        public void only_the_latest_is_kept()
        {
            Environment.ProjectRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(1)
                .ShouldHaveAll(v => v.Version.Equals(LionelVersion));
        }

        [Test]
        public void reports_success()
        {
            Results.ShouldHaveAtLeastOne(x => x is Success);
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNo(x => x is Error);
        }
    }

    public class cleaning_all_wraps : context.command_context<CleanWrapCommand>
    {
        static readonly Version LionelVersion = new Version(1, 0, 0, 123);
        static readonly Version RichieVersion = new Version(2, 2, 0, 456);

        public cleaning_all_wraps()
        {
            given_project_repository();
            given_project_package("lionel", new Version(1, 0, 0, 0));
            given_project_package("lionel", LionelVersion);
            given_project_package("richie", new Version(2, 2, 0, 0));
            given_project_package("richie", RichieVersion);
            when_executing_command("-all", "-Project");
        }

        [Test]
        public void all_wraps_have_only_their_latest_version_maintained()
        {
            Environment.ProjectRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(1)
                .ShouldHaveAll(v => v.Version.Equals(LionelVersion));

            Environment.ProjectRepository
    .PackagesByName["richie"]
    .ShouldHaveCountOf(1)
    .ShouldHaveAll(v => v.Version.Equals(RichieVersion));
        }

        [Test]
        public void reports_success()
        {
            Results.ShouldHaveAtLeastOne(x => x is Success);
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNo(x => x is Error);
        }
    }

    public class cleaning_package_from_system_repository_only : context.command_context<CleanWrapCommand>
    {
        static readonly Version LionelVersion = new Version(1, 0, 0, 123);

        public cleaning_package_from_system_repository_only()
        {
            given_project_repository();
            given_project_package("lionel", new Version(1, 0, 0, 0));
            given_project_package("lionel", LionelVersion);

            given_system_package("lionel", new Version(1, 0, 0, 0));
            given_system_package("lionel", LionelVersion);

            when_executing_command("lionel", "-system");
        }

        [Test]
        public void the_package_is_cleaned_from_the_system_repository()
        {
            Environment.SystemRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(1)
                .ShouldHaveAll(v => v.Version.Equals(LionelVersion));
        }

        [Test]
        public void the_project_repository_should_remain_the_same()
        {
            Environment.ProjectRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(2);
        }

        [Test]
        public void reports_success()
        {
            Results.ShouldHaveAtLeastOne(x => x is Success);
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNo(x => x is Error);
        }
    }

    public class cleaning_package_from_both_repositories : context.command_context<CleanWrapCommand>
    {
        static readonly Version LionelVersion = new Version(1, 0, 0, 123);

        public cleaning_package_from_both_repositories()
        {
            given_project_repository();
            given_project_package("lionel", new Version(1, 0, 0, 0));
            given_project_package("lionel", LionelVersion);

            given_system_package("lionel", new Version(1, 0, 0, 0));
            given_system_package("lionel", LionelVersion);

            when_executing_command("lionel");
        }

        [Test]
        public void the_package_is_cleaned_from_the_system_repository()
        {
            Environment.SystemRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(1)
                .ShouldHaveAll(v => v.Version.Equals(LionelVersion));
        }

        [Test]
        public void the_package_is_cleaned_from_the_project_repository()
        {
            Environment.ProjectRepository
                .PackagesByName["lionel"]
                .ShouldHaveCountOf(1)
                .ShouldHaveAll(v => v.Version.Equals(LionelVersion));
        }

        [Test]
        public void reports_success()
        {
            Results.ShouldHaveAtLeastOne(x => x is Success);
        }

        [Test]
        public void reported_no_errors()
        {
            Results.ShouldHaveNo(x => x is Error);
        }
    }

}
