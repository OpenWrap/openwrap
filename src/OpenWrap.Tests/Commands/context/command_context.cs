using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Tests.Commands.context
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
            if (Environment.ProjectRepository == null)
                Environment.ProjectRepository = new InMemoryRepository();
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

        protected void given_project_repository()
        {
            Environment.ProjectRepository = new InMemoryRepository();
        }
    }
}