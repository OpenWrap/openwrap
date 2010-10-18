using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using OpenWrap.Dependencies;

namespace OpenWrap.Tests.Commands.Wrap
{
    class set_wrap_content_true : context.command_context<SetWrapCommand>
    {
        public set_wrap_content_true()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", new Version("1.0.0.0"));

            when_executing_command("sauron", "-content", "true");
        }

        [Test]
        public void dependency_content_is_true()
        {
            Environment.Descriptor.Dependencies.First().ContentOnly.ShouldBeTrue();
        }

        [Test]
        public void dependency_anchored_unchanged()
        {
            Environment.Descriptor.Dependencies.First().Anchored.ShouldBeFalse();
        }
    }

    class set_wrap_anchored_true : context.command_context<SetWrapCommand>
    {
        public set_wrap_anchored_true()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", new Version("1.0.0.0"));
            
            when_executing_command("sauron", "-anchored", "true");
        }

        [Test]
        public void dependency_anchored_is_true()
        {
            Environment.Descriptor.Dependencies.First().Anchored.ShouldBeTrue();
        }

        [Test]
        public void dependency_contentonly_unchanged()
        {
            Environment.Descriptor.Dependencies.First().ContentOnly.ShouldBeFalse();
        }
    }

    class set_wrap_anchored_false : context.command_context<SetWrapCommand>
    {
        public set_wrap_anchored_false()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", new Version("1.0.0.0"));
            ((InMemoryPackage)Environment.ProjectRepository.PackagesByName["sauron"].First()).Anchored = true;

            when_executing_command("sauron", "-anchored", "false");
        }

        [Test]
        public void dependency_anchored_is_false()
        {
            Environment.Descriptor.Dependencies.First().Anchored.ShouldBeFalse();
        }
    }

    class set_wrap_version_to_2 : context.command_context<SetWrapCommand>
    {
        public set_wrap_version_to_2()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", new Version("1.0.0.0"));

            when_executing_command("sauron", "-version", "2.0.0");
        }

        [Test]
        public void dependency_has_one_version_vertex()
        {
            var vertices = Environment.Descriptor.Dependencies.First().VersionVertices;
            vertices.ShouldHaveCountOf(1);
        }

        [Test]
        public void dependency_version_equals_2()
        {
            var vertices = Environment.Descriptor.Dependencies.First().VersionVertices;
            vertices.First().Version.Equals(new Version("2.0.0"));
        }
    }

    class set_wrap_version_to_any : context.command_context<SetWrapCommand>
    {
        public set_wrap_version_to_any()
        {
            given_dependency("depends: sauron = 1.0.0");
            given_project_package("sauron", new Version("1.0.0.0"));

            when_executing_command("sauron", "-anyversion");
        }

        [Test]
        public void dependency_version_is_any()
        {
            var vertices = Environment.Descriptor.Dependencies.First().VersionVertices;
            vertices.First().ShouldBeOfType<AnyVersionVertex>();
        }
    }
}
