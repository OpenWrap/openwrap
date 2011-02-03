using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Commands;
using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    public class remove_from_project_when_not_in_project : context.remove_wrap
    {
        public remove_from_project_when_not_in_project()
        {
            given_project_repository(null);
            when_executing_command("saruman");
        }
        [Test]
        public void an_error_is_triggered()
        {
            Results.ShouldHaveError();
        }
    }
    public class remove_unknown_project_package : context.remove_wrap
    {
        public remove_unknown_project_package()
        {
            when_executing_command("saruman");
        }
        [Test]
        public void an_error_is_triggered()
        {
            Results.ShouldHaveError();
        }
    }
    public class remove_unknown_system_package : context.remove_wrap
    {
        public remove_unknown_system_package()
        {
            when_executing_command("saruman", "-system");
        }
        [Test]
        public void an_error_is_triggered()
        {
            Results.ShouldHaveError();
        }
    }
    public class removing_last_version : context.remove_wrap
    {
        public removing_last_version()
        {
            given_system_package("saruman", "1.0.0.0");
            given_system_package("saruman", "1.0.0.1");
            when_executing_command("saruman","-system", "-last");
        }
        [Test]
        public void packge_is_removed()
        {
            Environment.SystemRepository.ShouldNotHavePackage("saruman", "1.0.0.1");
        }
        [Test]
        public void earlier_versions_are_preserved()
        {
            Environment.SystemRepository.ShouldHavePackage("saruman", "1.0.0.0");
        }
    }
    public class removing_wrap_by_version_and_last : context.remove_wrap
    {
        public removing_wrap_by_version_and_last()
        {
            given_system_package("saruman", "1.0.0.0");
            when_executing_command("saruman", "-version", "1.0.0.0", "-last");
        }
        [Test]
        public void error_is_displayed()
        {
            Results.ShouldHaveError();
        }
    }
    public class removing_wrap_by_version_in_system : context.remove_wrap
    {
        public removing_wrap_by_version_in_system()
        {
            given_system_package("saruman", "1.0.0.0");
            given_system_package("saruman", "1.0.0.1");
            when_executing_command("saruman", "-system", "-version", "1.0.0.1");
        }
        [Test]
        public void version_is_removed()
        {
            Environment.SystemRepository
                    .ShouldNotHavePackage("saruman", "1.0.0.1");
        }
        [Test]
        public void other_versions_not_removed()
        {
            Environment.SystemRepository
                    .ShouldHavePackage("saruman", "1.0.0.0");
        }
    }
    public class removing_wrap_by_version_in_project : context.remove_wrap
    {
        public removing_wrap_by_version_in_project()
        {
            given_dependency("depends: saruman");
            given_project_package("saruman", "1.0.0.0");
            given_project_package("saruman", "1.0.0.1");
            when_executing_command("saruman", "-project", "-version", "1.0.0.0");
        }
        [Test]
        public void version_is_removed()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("saruman", "1.0.0.0");
        }
        [Test]
        public void other_versions_are_not_removed()
        {
            Environment.ProjectRepository.ShouldHavePackage("saruman", "1.0.0.1");
        }
        [Test]
        public void descriptor_is_not_updated()
        {
            Environment.Descriptor.Dependencies.ShouldHaveCountOf(1);
        }
        [Test]
        public void warning_is_issued_about_descriptor_not_updated()
        {
            Results.Where(x=>x.Type == CommandResultType.Warning)
                    .ShouldHaveCountOf(1);
        }
    }
    public class removing_wrap_by_name_in_both_system_and_proejct : context.remove_wrap
    {
        public removing_wrap_by_name_in_both_system_and_proejct()
        {
            given_dependency("depends: gandalf");
            given_system_package("gandalf", "1.0.0.0");
            given_project_package("saruman", "99");
            given_project_package("gandalf", "1.0.0.0");
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
            given_system_package("gandalf", "1.0.0.0");
            given_system_package("gandalf", "1.0.1.0");
            given_system_package("saruman", "99");
            given_project_package("gandalf", "1.0.0.0");
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
            given_project_package("foo", "1.0.0.0");
            given_project_package("bar", "1.0.0.0");

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

            protected IPackageDescriptor PostCommandDescriptor;
            protected override void when_executing_command(params string[] parameters)
            {
                base.when_executing_command(parameters);
                PostCommandDescriptor = new PackageDescriptorReaderWriter().Read(Environment.DescriptorFile);
            }
        }
    }
}
