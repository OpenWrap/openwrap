using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Commands
{
    class when_synchronizing_dependencies : context.command_context<SyncWrapCommand>
    {
        public when_synchronizing_dependencies()
        {
            given_dependency("depends rings-of-power");
            given_remote_package("sauron", new Version(1, 0, 0));
            given_remote_package("rings-of-power", new Version(1,0,0), "depends sauron");

            when_executing_command();
        }

        [Test]
        public void local_repository_has_new_packages()
        {

            Environment.ProjectRepository.PackagesByName["sauron"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));

            Environment.ProjectRepository.PackagesByName["rings-of-power"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));
        }

        [Test]
        public void user_repository_has_new_packages()
        {
            Environment.UserRepository.PackagesByName["sauron"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));

            Environment.UserRepository.PackagesByName["rings-of-power"]
                .ShouldHaveCountOf(1)
                .First().Version.ShouldBe(new Version(1, 0, 0));
        
        }
    }
    namespace context
    {
        public class command_context<T> : Testing.context
            where T:ICommand
        {
            protected CommandRepository Commands;
            protected AttributeBasedCommandDescriptor<T> Command;
            protected List<ICommandResult> Results;
            protected InMemoryEnvironment Environment;

            public command_context()
            {
                Command = new AttributeBasedCommandDescriptor<T>();
                Commands = new CommandRepository() { Command };
                Environment = new InMemoryEnvironment();

                WrapServices.Clear();
                WrapServices.RegisterService<IEnvironment>(Environment);
                WrapServices.RegisterService<IPackageManager>(new PackageManager());
                WrapServices.RegisterService<ICommandRepository>(Commands);
            }

            protected void given_dependency(string dependency)
            {
                new WrapDependencyParser().Parse(dependency, Environment.Descriptor);
            }
            protected void given_remote_package(string name, Version version, params string[] dependencies)
            {
                AddPackage(Environment.RemoteRepository, name, version, dependencies);
            }
            protected void given_user_package(string name, Version version, params string[] dependencies)
            {
                AddPackage(Environment.UserRepository, name, version, dependencies);
            }
            protected void given_project_package(string name, Version version, params string[] dependencies)
            {
                AddPackage(Environment.ProjectRepository, name, version, dependencies);
            }

            static void AddPackage(InMemoryRepository repository, string name, Version version, string[] dependencies)
            {
                repository.Packages.Add(new InMemoryPackage
                {
                    Name = name,
                    Version = version,
                    Source = repository,
                    Dependencies = dependencies.Select(x =>
                                                       WrapDependencyParser.ParseDependency(x)).ToList()
                });
            }

            protected virtual void when_executing_command(params string[] parameters)
            {
                var allParams = new[] { Command.Noun, Command.Verb }.Concat(parameters);
                Results = new CommandLineProcessor(Commands).Execute(allParams).ToList();
            }
        }
        public class InMemoryEnvironment : IEnvironment
        {
            public InMemoryRepository ProjectRepository;
            public IList<InMemoryRepository> RemoteRepositories;
            public InMemoryRepository UserRepository;
            public InMemoryRepository RemoteRepository;

            public InMemoryEnvironment()
            {
                ProjectRepository = new InMemoryRepository();
                UserRepository = new InMemoryRepository();
                RemoteRepository = new InMemoryRepository();
                RemoteRepositories = new List<InMemoryRepository> { RemoteRepository };
                Descriptor = new WrapDescriptor();

            }

            void IService.Initialize()
            {
            }

            IPackageRepository IEnvironment.ProjectRepository
            {
                get { return ProjectRepository; }
            }

            public WrapDescriptor Descriptor { get; set; }

            IEnumerable<IPackageRepository> IEnvironment.RemoteRepositories
            {
                get { return RemoteRepositories.Cast<IPackageRepository>(); }
             
            }

            IPackageRepository IEnvironment.UserRepository
            {
                get { return UserRepository; }
            }
        }
    }
}
