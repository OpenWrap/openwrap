using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement.Packages
{
    public class InMemoryPackage : IPackageInfo, IPackage
    {
        readonly Dictionary<string, List<Exports.IFile>> _content = new Dictionary<string, List<Exports.IFile>>();

        public InMemoryPackage(IPackageInfo packageToCopy)
            : this()
        {
            Created = packageToCopy.Created;
            Dependencies = packageToCopy.Dependencies.ToList();
            Anchored = packageToCopy.Anchored;
            Description = packageToCopy.Description;
            Name = packageToCopy.Name;
            Nuked = packageToCopy.Nuked;
            Source = packageToCopy.Source;
            Title = packageToCopy.Title;
            SemanticVersion = packageToCopy.SemanticVersion;
        }

        public InMemoryPackage()
        {
            Created = DateTime.Now;
            Dependencies = new List<PackageDependency>();
            Descriptor = new PackageDescriptor();
        }

        [Obsolete("Plase use SemanticVersion")]
        public Version Version
        {
            get { return SemanticVersion != null ? SemanticVersion.ToVersion() : null; }
        }
        public bool Anchored { get; set; }

        public IEnumerable<IGrouping<string, Exports.IFile>> Content
        {
            get { return _content.SelectMany(x => x.Value.GroupBy(_ => x.Key)); }
        }

        public DateTimeOffset Created { get; private set; }
        public ICollection<PackageDependency> Dependencies { get; set; }

        public string Description { get; set; }
        public IPackageDescriptor Descriptor { get; private set; }

        public string FullName
        {
            get { return Name + "-" + SemanticVersion; }
        }

        public PackageIdentifier Identifier
        {
            get { return new PackageIdentifier(Name, SemanticVersion); }
        }

        public bool IsValid
        {
            get { return true; }
        }

        public string Name { get; set; }
        public string Namespace { get; set; }
        public bool Nuked { get; set; }

        public IPackageRepository Source { get; set; }
        public string Title { get; set; }
        public SemanticVersion SemanticVersion { get; set; }

        public ICollection<Exports.IFile> this[string exportName]
        {
            get
            {
                List<Exports.IFile> outValue;
                if (!_content.TryGetValue(exportName, out outValue))
                    _content[exportName] = outValue = new List<Exports.IFile>();
                return outValue;
            }
        }

        public Stream OpenStream()
        {
            var package = new InMemoryFile(@"c:\test.wrap");
            Packager.NewWithDescriptor(package, Name, SemanticVersion.ToString(), Dependencies.Select(x => "depends: " + x).ToArray());
            return package.OpenRead();
        }

        public IPackage Load()
        {
            return this;
        }
    }
}