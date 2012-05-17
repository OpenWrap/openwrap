﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace Tests.contexts
{
    public abstract class assembly_resolving : openwrap_context
    {
        protected IEnumerable<Exports.IAssembly> AssemblyReferences;
        readonly ITemporaryDirectory TempDirectory;

        public assembly_resolving()
        {
            given_project_repository(new FolderRepository(FileSystem.GetTempDirectory().GetDirectory(Guid.NewGuid().ToString()).MustExist(), FolderRepositoryOptions.SupportLocks));
            TempDirectory = FileSystem.CreateTempDirectory();
        }


        protected StrategyResolver PackageResolver { get; set; }

        [TestFixtureTearDown]
        public void Dispose()
        {
            TempDirectory.Delete();
        }

        protected IEnumerable<PackageContent> Assemblies(params PackageContent[] assemblies)
        {
            return assemblies;
        }

        protected PackageContent Assembly(string assemblyName, string relativePath)
        {
            var assemblyFile = TempDirectory.CreateAssemblyStream(assemblyName);
            return new PackageContent { FileName = assemblyFile.Name, RelativePath = relativePath, Stream = () => assemblyFile.OpenRead(), Size = assemblyFile.Size };
        }

        protected InMemoryFile Package(string name, string version, IEnumerable<PackageContent> content, params string[] descriptorLines)
        {
            var file = new InMemoryFile(name + "-" + version + ".wrap");
            Packager.NewWithDescriptor(file, name, version, content, descriptorLines);
            return file;
        }

        protected void given_environment(string platform, string profile)
        {
            Environment.ExecutionEnvironment = new ExecutionEnvironment(platform, profile);
        }

        protected void given_project_package(string name, string version, IEnumerable<PackageContent> content, params string[] descriptorLines)
        {
            var packageFile = Package(name, version, content, descriptorLines);
            var packageStream = packageFile.OpenRead();
            using (var publisher = ((ISupportPublishing)Environment.ProjectRepository).Publisher())
            using (var readStream = packageStream)
                publisher.Publish(packageFile.Name, readStream);
        }

        protected void when_resolving_assemblies()
        {
            AssemblyReferences = ServiceLocator.GetService<IPackageManager>()
                .GetProjectAssemblyReferences(Environment.Descriptor, Environment.ProjectRepository, Environment.ExecutionEnvironment, false)
                .ToList();
        }
    }
}