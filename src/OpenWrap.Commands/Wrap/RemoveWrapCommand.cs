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
    public class RemoveWrapCommand : ICommand
    {
        // TODO: Need to be able to remove packages from the system repository
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

        public IEnumerable<ICommandOutput> Execute()
        {
            if (Project)
                foreach(var m in RemoveFromProjectRepository()) yield return m;
            if (System)
                foreach(var m in RemoveFromSystemRepository()) yield return m;
        }

        IEnumerable<ICommandOutput> RemoveFromSystemRepository()
        {
            var systemRepository = (ISupportCleaning)Environment.SystemRepository;
            
            return systemRepository.Clean(systemRepository.PackagesByName
                                           .SelectMany(x => x)
                                           .Where(PackageShouldBeKept))
                                   .Select(x=>PackageRemovedMessage(x));
        }

        ICommandOutput PackageRemovedMessage(PackageCleanResult packageCleanResult)
        {
            if (packageCleanResult.Success)
                return new GenericMessage("Package '{0}' removed.", packageCleanResult.Package.FullName);
            return new Warning("Package '{0}' could not be removed.", packageCleanResult.Package.FullName);
        }

        bool PackageShouldBeKept(IPackageInfo packageInfo)
        {
            return packageInfo.Name.EqualsNoCase(Name) == false;
        }

        IEnumerable<ICommandOutput> RemoveFromProjectRepository()
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

        static IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }
        }
    }
}
