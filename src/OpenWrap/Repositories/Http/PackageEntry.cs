using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;

namespace OpenWrap.Repositories.Http
{
    public class PackageEntry
    {
        public PackageEntry()
        {
            Dependencies = Enumerable.Empty<String>();
            Authors = Enumerable.Empty<String>();
        }
        public DateTimeOffset CreationTime { get; set; }
        public IEnumerable<string> Dependencies { get; set; }

        public string Description { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public bool Nuked { get; set; }
        public Uri PackageHref { get; set; }
        public SemanticVersion Version { get; set; }

        public DateTimeOffset UpdateTime { get; set; }
    }
    public class PackageEntryWrapper : IPackageInfo
    {
        PackageEntry _entry;
        readonly Func<IPackage> _load;

        public PackageEntryWrapper(IPackageRepository source, PackageEntry entry, Func<IPackage> load)
        {
            Source = source;
            _entry = entry;
            _load = load;
            Identifier = new PackageIdentifier(_entry.Name, _entry.Version);
            Dependencies = entry.Dependencies.Select(DependsParser.ParseDependsValue).ToList();
        }

        [Obsolete("Please use SemanticVersion")]
        public Version Version
        {
            get { return SemanticVersion != null ? SemanticVersion.ToVersion() : null; }
        }
        public PackageIdentifier Identifier { get; private set; }
        public ICollection<PackageDependency> Dependencies { get; private set; }
        public string Name { get { return _entry.Name; } }
        public SemanticVersion SemanticVersion { get { return _entry.Version; } }
        public IPackage Load()
        {
            return _load();
        }

        public IPackageRepository Source { get; private set; }
        public string FullName { get { return _entry.Name + "-" + _entry.Version; } }
        public string Description { get { return _entry.Description; } }

        public string Title
        {
            get { return _entry.Title; }
        }

        public DateTimeOffset Created { get { return _entry.CreationTime; } }
        public bool Anchored { get { return false; } }
        public bool Nuked { get { return _entry.Nuked; } }

        public bool IsValid
        {
            get { return true; }
        }
    }
}