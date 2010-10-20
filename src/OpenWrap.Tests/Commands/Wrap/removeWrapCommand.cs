using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Commands.Wrap;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    public class removing_wrap_by_name_in_both_system_and_proejct : context.remove_wrap
    {
        public removing_wrap_by_name_in_both_system_and_proejct()
        {
            given_dependency("depends: gandalf");
            given_system_package("gandalf", "1.0.0.0".ToVersion());
            given_project_package("saruman", "99".ToVersion());
            given_project_package("gandalf", "1.0.0.0".ToVersion());
            when_executing_command("gandalf", "-project", "-system");
        }
        [Test]
        public void package_removed_from_both_repositories()
        {
            PostCommandDescriptor.Dependencies
                .Where(x => x.Name == "galdalf")
                .ShouldBeEmpty();
            Environment.SystemRepository.PackagesByName["gandalf"]
                    .ShouldBeEmpty();
        }
    }
    public class removing_system_wrap_by_name : context.remove_wrap
    {
        public removing_system_wrap_by_name()
        {
            given_system_package("gandalf", "1.0.0.0".ToVersion());
            given_system_package("gandalf", "1.0.1.0".ToVersion());
            given_system_package("saruman", "99".ToVersion());
            given_project_package("gandalf", "1.0.0.0".ToVersion());
            when_executing_command("gandalf", "-system");
        }
        [Test]
        public void package_is_removed_from_system()
        {
            Environment.SystemRepository.PackagesByName["gandalf"]
                    .ShouldBeEmpty();
        }
        [Test]
        public void package_with_different_name_is_not_removed_from_system()
        {
            Environment.SystemRepository.PackagesByName["saruman"]
                    .ShouldHaveCountOf(1);
        }
        [Test]
        public void project_repository_is_unaffected()
        {
            Environment.ProjectRepository.ShouldHavePackage("gandalf", "1.0.0.0");
        }
    }
    public class removing_project_wrap_by_name : context.remove_wrap
    {
        public removing_project_wrap_by_name()
        {
            given_dependency("depends: bar");
            given_dependency("depends: foo");
            given_project_package("foo", "1.0.0.0".ToVersion());
            given_project_package("bar", "1.0.0.0".ToVersion());

            when_executing_command("foo");
        }


        [Test]
        public void dependency_is_removed_from_descriptor()
        {
            PostCommandDescriptor.Dependencies.ShouldHaveCountOf(1);
        }

        [Test]
        public void package_removed_from_descriptor()
        {
            PostCommandDescriptor.Dependencies.Where(x=>x.Name == "foo").ShouldBeEmpty();
        }

        [Test]
        public void unaffected_packages_remain()
        {
            PostCommandDescriptor.Dependencies.Where(x => x.Name != "foo").ShouldHaveCountOf(1);
        }
    }

    namespace context
    {
        public abstract class remove_wrap : command_context<RemoveWrapCommand>
        {

            protected PackageDescriptor PostCommandDescriptor;
            protected override void when_executing_command(params string[] parameters)
            {
                base.when_executing_command(parameters);
                PostCommandDescriptor = new PackageDescriptorReaderWriter().Read(Environment.DescriptorFile);
            }
        }
    }
}
