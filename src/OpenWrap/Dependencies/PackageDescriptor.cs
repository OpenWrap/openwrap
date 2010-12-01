using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    //TODO: As order is important in resolving dependencies, update this code to keep track of lists.
    public class PackageDescriptor : IPackageInfo
    {
        public PackageDescriptor(PackageDescriptor copy)
        {
            Dependencies = copy.Dependencies.Select(x => new PackageDependency(x)).ToList();
            Overrides = new List<PackageNameOverride>(copy.Overrides);
            Description = copy.Description;
            UseProjectRepository = copy.UseProjectRepository;
            Name = copy.Name;
            Version = copy.Version;
            Anchored = copy.Anchored;
            BuildCommand = copy.BuildCommand;
            CreationTime = copy.CreationTime;
        }
        public PackageDescriptor()
        {
            Dependencies = new List<PackageDependency>();
            Overrides = new List<PackageNameOverride>();
            Description = "";
            UseProjectRepository = true;
            CreationTime = DateTimeOffset.UtcNow;
        }

        public PackageIdentifier Identifier { get { return new PackageIdentifier(Name, Version); } }

        public ICollection<PackageDependency> Dependencies { get; set; }
        public ICollection<PackageNameOverride> Overrides { get; set; }

        public string Name { get; set; }

        public Version Version { get; set; }

        public string Description { get; set; }
        public bool UseProjectRepository { get; set; }

        public bool Nuked { get { return false; } }

        public IPackage Load()
        {
            return null;
        }

        public IPackageRepository Source
        {
            get { return null; }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTimeOffset CreationTime { get; private set; }

        public bool Anchored { get; set; }

        public string BuildCommand { get; set; }
    }
}