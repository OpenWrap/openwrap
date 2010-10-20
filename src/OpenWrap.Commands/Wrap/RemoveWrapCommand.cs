using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "remove", Noun = "wrap")]
    public class RemoveWrapCommand : WrapCommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        bool? _project;

        [CommandInput]
        public bool Project
        {
            get { return _project ?? !System; }
            set { _project = value; }
        }

        [CommandInput]
        public bool System { get; set; }

        [CommandInput]
        public Version Version { get; set; }

        [CommandInput]
        public bool Last { get; set; }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(VerifyInputs()).Or(ExecuteCore());
        }

        IEnumerable<ICommandOutput> VerifyInputs()
        {
            if (Version != null && Last)
                yield return new Error("Cannot use '-Last' and '-Version' together.");
            if (System && !Environment.SystemRepository.PackagesByName[Name].Any())
                yield return new Error("Cannot find package named '{0}' in system repository.", Name);
            if (Project && Environment.ProjectRepository == null)
                yield return new Error("Not in a pacakge directory.");
        }

        IEnumerable<ICommandOutput> ExecuteCore()
        {
            if (Project)
                foreach(var m in RemoveFromProjectRepository()) yield return m;
            if (System)
                foreach(var m in RemoveFromSystemRepository()) yield return m;
            foreach(var m in  PackageManager.VerifyPackageCache(Environment, Environment.Descriptor))
                yield return m;
        }

        IEnumerable<ICommandOutput> RemoveFromSystemRepository()
        {
            var systemRepository = (ISupportCleaning)Environment.SystemRepository;

            return RemoveFromRepository(systemRepository);
        }

        IEnumerable<ICommandOutput> RemoveFromRepository(ISupportCleaning repository)
        {
            if (Last)
                Version = repository.PackagesByName[Name].Select(x=>x.Version)
                                                         .OrderByDescending(x => x)
                                                         .FirstOrDefault();
            return repository.Clean(repository.PackagesByName
                                                  .SelectMany(x => x)
                                                  .Where(PackageShouldBeKept))
                                                  .Select(PackageRemovedMessage);
        }

        ICommandOutput PackageRemovedMessage(PackageCleanResult packageCleanResult)
        {
            if (packageCleanResult.Success)
                return new GenericMessage("Package '{0}' removed.", packageCleanResult.Package.FullName);
            return new Warning("Package '{0}' could not be removed.", packageCleanResult.Package.FullName);
        }

        bool PackageShouldBeKept(IPackageInfo packageInfo)
        {
            bool matchesName = packageInfo.Name.EqualsNoCase(Name);
            bool matchesVersion = Version == null ? true : packageInfo.Version == Version;
            return !(matchesName && matchesVersion);
        }

        IEnumerable<ICommandOutput> RemoveFromProjectRepository()
        {
            return Version == null ? RemoveFromDescriptor() : RemovePackageFilesFromProjectRepo();
        }

        IEnumerable<ICommandOutput> RemovePackageFilesFromProjectRepo()
        {
            yield return
                    new Warning(
                            "You specified a version to remove from your project. Your descriptor will not be updated, and the package files will be removed. If you want to change what version of a pacakge you depend on, use the 'set-wrap' command.")
                    ;
            foreach (var m in RemoveFromRepository((ISupportCleaning)Environment.ProjectRepository))
                yield return m;
        }

        IEnumerable<ICommandOutput> RemoveFromDescriptor()
        {
            var dependency = FindProjectDependencyByName();
            if (dependency == null)
            {
                yield return new Error("Dependency not found: " + Name);
                yield break;
            }

            Environment.Descriptor.Dependencies.Remove(dependency);
            using (var destinationStream = Environment.DescriptorFile.OpenWrite())
                new PackageDescriptorReaderWriter().Write(Environment.Descriptor, destinationStream);
        }

        PackageDependency FindProjectDependencyByName()
        {
            return Environment.Descriptor != null
                           ? Environment.Descriptor.Dependencies.FirstOrDefault(d => d.Name.EqualsNoCase(Name))
                           : null;
        }

    }
}
