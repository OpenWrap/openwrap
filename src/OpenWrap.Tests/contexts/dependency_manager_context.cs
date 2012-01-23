using System;
using System.Linq;
using OpenWrap;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Repositories;

namespace Tests.contexts
{
    public abstract class dependency_resolver_context : OpenWrap.Testing.context
    {
        protected InMemoryRepository CurrentDirectoryRepository;
        protected PackageDescriptor DependencyDescriptor;
        protected InMemoryRepository ProjectRepository;
        protected InMemoryRepository RemoteRepository;
        protected DependencyResolutionResult Resolve;
        protected InMemoryRepository SystemRepository;

        public dependency_resolver_context()
        {
            DependencyDescriptor = new PackageDescriptor
            {
                    Name = "test",
                    SemanticVersion = "1.0".ToSemVer()
            };
            ProjectRepository = new InMemoryRepository("Local repository");
            SystemRepository = new InMemoryRepository("System repository");
            RemoteRepository = new InMemoryRepository("Remote repository");
            CurrentDirectoryRepository = new InMemoryRepository("Current repository");
        }

        protected void given_current_directory_package(string name, params string[] dependencies)
        {
            Add(CurrentDirectoryRepository, name, dependencies);
        }

        protected void given_dependency(string dependency)
        {
            new DependsParser().Parse(dependency, DependencyDescriptor);
        }

        protected void given_dependency_override(string from, string to)
        {
            DependencyDescriptor.Overrides.Add(new PackageNameOverride(from, to));
        }

        protected void given_project_package(string name, params string[] dependencies)
        {
            Add(ProjectRepository, name, dependencies);
        }

        protected void given_remote1_package(string name, params string[] dependencies)
        {
            Add(RemoteRepository, name, dependencies);
        }

        protected void given_system_package(string name, params string[] dependencies)
        {
            Add(SystemRepository, name, dependencies);
        }

        protected void when_resolving_packages()
        {
            Resolve = new ExhaustiveResolver()
                    .TryResolveDependencies(DependencyDescriptor,
                                            new[]
                                            {
                                                    CurrentDirectoryRepository,
                                                    ProjectRepository,
                                                    SystemRepository,
                                                    RemoteRepository
                                            });
        }

        void Add(InMemoryRepository repository, string name, string[] dependencies)
        {
            var package = new InMemoryPackage
            {
                    Name = PackageNameUtility.GetName(name),
                    SemanticVersion = PackageNameUtility.GetVersion(name),
                    Source = repository,
                    Dependencies = dependencies.SelectMany(x => DependsParser.ParseDependsInstruction(x).Dependencies)
                            .ToList()
            };
            repository.Packages.Add(package);
        }

    }
}