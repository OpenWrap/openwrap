using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.add_wrap.pre
{
    public class without_flag : contexts.add_wrap
    {
        public without_flag()
        {
            given_project_repository();
            given_remote_package("sauron", "1.0.0");
            given_remote_package("sauron", "2.0.0-beta");
            when_executing_command("sauron");
        }

        [Test]
        public void non_beta_should_be_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "1.0.0".ToSemVer());
        }
    }
    public class with_flag : contexts.add_wrap
    {
        public with_flag()
        {
            given_project_repository();
            given_remote_package("sauron", "1.0.0");
            given_remote_package("sauron", "2.0.0-beta");
            when_executing_command("sauron -edge");
        }

        [Test]
        public void beta_should_be_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "2.0.0-beta".ToSemVer());
        }
    }
    public class no_flag_nested_edge_dependency : contexts.add_wrap
    {
        public no_flag_nested_edge_dependency()
        {
            given_project_repository();
            given_remote_package("one-ring", "2.0.0-beta");
            given_remote_package("one-ring", "1.0.0");
            given_remote_package("sauron", "2.0.0", "depends: one-ring edge");
            given_remote_package("sauron", "1.0.0", "depends: one-ring");
            when_executing_command("sauron");
        }

        [Test]
        public void release_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "1.0.0".ToSemVer());
        }

        [Test]
        public void release_dependency_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "one-ring", "1.0.0".ToSemVer());

        }
    }
    public class with_flag_nested_edge_dependency : contexts.add_wrap
    {
        public with_flag_nested_edge_dependency()
        {
            given_project_repository();
            given_remote_package("one-ring", "2.0.0-beta");
            given_remote_package("one-ring", "1.0.0");
            given_remote_package("sauron", "2.0.0", "depends: one-ring edge");
            given_remote_package("sauron", "1.0.0", "depends: one-ring");
            when_executing_command("sauron -edge");
        }

        [Test]
        public void release_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "2.0.0".ToSemVer());
        }

        [Test]
        public void release_dependency_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "one-ring", "2.0.0-beta".ToSemVer());

        }
    }
    public class no_flag_all_edge_dependencies : contexts.add_wrap
    {
        public no_flag_all_edge_dependencies()
        {
            given_project_repository();
            given_remote_package("one-ring", "2.0.0-beta");
            given_remote_package("one-ring", "1.0.0");
            given_remote_package("sauron", "2.0.0-beta", "depends: one-ring edge");
            given_remote_package("sauron", "1.0.0", "depends: one-ring");
            when_executing_command("sauron");
        }

        [Test]
        public void release_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "1.0.0".ToSemVer());
        }

        [Test]
        public void release_dependency_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "one-ring", "1.0.0".ToSemVer());

        }
    }

    public class with_flag_latest_is_release : contexts.add_wrap
    {
        public with_flag_latest_is_release()
        {
            given_project_repository();
            given_remote_package("sauron", "2.0.0-beta");
            given_remote_package("sauron", "2.0.0");
            when_executing_command("sauron -edge");
        }

        [Test]
        public void release_is_used()
        {
            package_is_in_repository(Environment.ProjectRepository, "sauron", "2.0.0".ToSemVer());
        }
        }

}