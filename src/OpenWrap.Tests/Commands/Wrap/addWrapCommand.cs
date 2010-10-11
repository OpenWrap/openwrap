using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class adding_wrap_twice : context.command_context<AddWrapCommand>
    {
        public adding_wrap_twice()
        {
            given_dependency("depends: sauron");
            given_project_package("sauron", new Version("1.0.0.0"));

            when_executing_command("sauron", "-content");
        }
        [Test]
        public void one_entry_exists()
        {
            Environment.Descriptor.Dependencies.Select(x => x.Name == "sauron")
                    .ShouldHaveCountOf(1);
        }
        [Test]
        public void entry_is_updated()
        {
            Environment.Descriptor.Dependencies.Single()
                    .ContentOnly.ShouldBeTrue();
        }
    }
    class adding_wrap_with_incompatible_arguments : context.command_context<AddWrapCommand>
    {
        public adding_wrap_with_incompatible_arguments()
        {
            given_project_repository(new InMemoryRepository("Project repository"));

            when_executing_command("-System", "-Project");
        }
        [Test]
        public void results_in_an_error()
        {
            Results.ShouldHaveAtLeastOne(x => x.Success() == false);
        }
    }
    class adding_wrap_from_system_pacakge_with_outdated_version_in_remote : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_system_pacakge_with_outdated_version_in_remote()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_project_package("sauron", new Version("1.0.0.0"));
            given_system_package("sauron", new Version("1.0.0.2"));
            given_remote_package("sauron", new Version("1.0.0.1"));

            when_executing_command("sauron");
        }
        [Test]
        public void latest_version_of_package_is_added()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "1.0.0.2");
        }
        [Test]
        public void outdated_version_is_not_added()
        {
            Environment.ProjectRepository.ShouldNotHavePackage("sauron", "1.0.0.1");
        }
    }
    class adding_wrap_from_local_package_in_project_path_without_descriptor_update : context.command_context<AddWrapCommand>
    {
        
        public adding_wrap_from_local_package_in_project_path_without_descriptor_update()
        {
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package("sauron", new Version(1, 0, 0));


            when_executing_command("-Name","sauron", "-nodesc");
        }

        [Test]
        public void the_package_is_installed_on_project_repository()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }

        [Test]
        public void descriptor_is_not_updated()
        {
            Environment.Descriptor.Dependencies.FirstOrDefault(x=>x.Name == "sauron")
                .ShouldBeNull();
        }
    }
    class adding_wrap_from_local_package_in_project_path : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_local_package_in_project_path()
        {
            given_dependency("depends: sauron");
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package("sauron", new Version(1, 0, 0));


            when_executing_command("-Name", "sauron");
        }

        [Test]
        public void the_package_is_installed_on_project_repository()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }

        [Test]
        public void the_package_is_installed_on_system_repository()
        {
            Environment.SystemRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }
    }

    class adding_wrap_from_local_package_in_project_path_with_system_parameter : context.command_context<AddWrapCommand>
    {
        string SAURON_NAME = "sauron";
        Version SAURON_VERSION = new Version(1, 0, 0);

        public adding_wrap_from_local_package_in_project_path_with_system_parameter()
        {
            given_dependency("depends: sauron");
            given_project_repository(new InMemoryRepository("Project repository"));
            given_currentdirectory_package(SAURON_NAME, SAURON_VERSION);


            when_executing_command("-Name", SAURON_NAME, "-System");
        }
        [Test]
        public void installs_package_in_system_repository()
        {
            package_is_in_repository(Environment.SystemRepository, SAURON_NAME, SAURON_VERSION);
        }

        [Test]
        public void doesnt_install_package_in_project_repository()
        {
            package_is_not_in_repository(Environment.ProjectRepository, SAURON_NAME, SAURON_VERSION);
        }
    }
    class adding_wrap_from_local_package_in_project_path_with_project_only_parameter : context.command_context<AddWrapCommand>
    {
        string SAURON_NAME = "sauron";
        Version SAURON_VERSION = new Version(1, 0, 0);
        public adding_wrap_from_local_package_in_project_path_with_project_only_parameter()
        {
            given_currentdirectory_package(SAURON_NAME, SAURON_VERSION);
            given_project_repository(new InMemoryRepository("Project repository"));

            when_executing_command("-Name", "sauron", "-Project");
        }
        [Test]
        public void installs_package_in_project_repository()
        {
            package_is_in_repository(Environment.ProjectRepository, SAURON_NAME, SAURON_VERSION);
        }
        [Test]
        public void doesnt_install_package_in_system_repository()
        {
            package_is_not_in_repository(Environment.SystemRepository, SAURON_NAME, SAURON_VERSION);
        }
    }
    class adding_wrap_from_local_package_outside_of_project_path : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_local_package_outside_of_project_path()
        {
            given_currentdirectory_package("sauron", new Version(1, 0, 0));

            when_executing_command("-Name", "sauron");
        }
        [Test]
        public void installs_package_in_system_repository()
        {
            Environment.SystemRepository.PackagesByName["sauron"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));
        }
        [Test]
        public void command_is_successful()
        {
            Results.ShouldHaveAll(x => x.Success());
        }
    }

    class adding_wrap_from_local_path_with_dependency : context.command_context<AddWrapCommand>
    {
        public adding_wrap_from_local_path_with_dependency()
        {
            given_project_repository(new InMemoryRepository("Project repository"));

            given_currentdirectory_package("sauron", new Version(1,0,0), "depends: one-ring");
            given_system_package("one-ring", new Version(1,0,0));

            when_executing_command("sauron");
        }

        [Test]
        public void package_is_installed()
        {
            Environment.ProjectRepository.PackagesByName["sauron"].ShouldHaveCountOf(1);
        }

        [Test]
        public void package_dependency_is_installed()
        {
            Environment.ProjectRepository.PackagesByName["one-ring"].ShouldHaveCountOf(1);

        }
    }

    class adding_non_existant_wrap : context.command_context<AddWrapCommand>
    {
        public adding_non_existant_wrap()
        {
            given_currentdirectory_package("sauron", new Version(1, 0, 0));
            when_executing_command("-Name", "saruman");
        }
        [Test]
        public void package_installation_is_unsuccessfull()
        {
            Results.ShouldHaveAtLeastOne(x => x.Success() == false);
        }
    }

    class adding_dependency_already_present : context.command_context<AddWrapCommand>
    {
        public adding_dependency_already_present()
        {
            given_dependency("depends: sauron >= 2.0");
            given_project_package("sauron", new Version(1,0,0));
            given_system_package("sauron", new Version(2,0,0));

            when_executing_command("sauron");
        }
        [Test]
        public void package_is_updated()
        {
            Environment.ProjectRepository.ShouldHavePackage("sauron", "2.0.0");
        }
    }


    public class adding_anchored_dependency : context.add_wrap_context
    {
        public adding_anchored_dependency()
        {
            given_file_based_project_repository();

            given_system_package("sauron", new Version(1, 0, 0));

            when_executing_command("sauron", "-anchored");
        }
        [Test]
        public void link_is_created()
        {
            ProjectRepositoryDir.GetDirectory("sauron")
                .Check(x=>x.Exists.ShouldBeTrue())
                .Check(x=>x.IsHardLink.ShouldBeTrue());
        }
        [Test]
        public void link_points_to_correct_path()
        {
            ProjectRepositoryDir.GetDirectory("sauron")
                    .Target.ShouldBe(ProjectRepositoryDir.GetDirectory("_cache").GetDirectory("sauron-1.0.0"));
        }
    }
    namespace context
    {
        public class add_wrap_context : context.command_context<AddWrapCommand>
        {
            protected IDirectory ProjectRepositoryDir;

            protected void given_file_based_project_repository()
            {
                ProjectRepositoryDir = FileSystem.GetDirectory(@"c:\repo");
                given_project_repository(new FolderRepository(ProjectRepositoryDir) { EnableAnchoring=true });
            }        
        }
    
    }
}
