using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.PackageModel.Parsers;

namespace OpenWrap.PackageModel
{
    public partial class PackageDescriptor : IPackageDescriptor
    {
        readonly PackageDescriptorEntryCollection _entries = new PackageDescriptorEntryCollection();
        SingleBoolValue _anchored;
        SingleDateTimeOffsetValue _created;
        PackageDependencyCollection _dependencies;
        SingleStringValue _description;
        SingleStringValue _name;
        PackageNameOverrideCollection _overrides;
        SingleBoolValue _useProjectRepository;
        SingleBoolValue _useSymLinks;
        SingleSemanticVersionValue _semanticVersion;
        SingleStringValue _referencedAssemblies;
        MultiLine<string> _buildCommands;
        MultiLine<string> _directoryStructure;
        MultiLine<string> _assemblyInfo;
        SingleStringValue _copyright;
        SingleStringValue _title;
        SingleStringValue _namespace;
        SingleBoolValue _storePackages;
        MultiLine<string> _author;
        SingleStringValue _buildConfiguration;
        SingleStringValue _trademark;
        MultiLine<string> _maintainer;
        SingleVersionValue _version;

        public ICollection<string> Maintainer
        {
            get { return _maintainer; }
        }

        public string Trademark
        {
            get { return _trademark.Value; }
            set { _trademark.Value = value; }
        }

        public PackageDescriptor(IEnumerable<IPackageDescriptorEntry> entries)
        {
            foreach (IPackageDescriptorEntry line in entries)
                Entries.Append(line);
            InitializeHeaders();
        }

        public PackageDescriptor()
        {
            InitializeHeaders();
        }

        public bool Anchored
        {
            get { return _anchored.Value; }
            set { _anchored.Value = value; }
        }


        public virtual DateTimeOffset Created
        {
            get { return _created.Value; }
            private set { _created.Value = value; }
        }

        public ICollection<string> Build
        {
            get { return _buildCommands; }
        }

        public ICollection<PackageDependency> Dependencies
        {
            get { return _dependencies; }
        }

        public string Title
        {
            get { return _title.Value; }
            set { _title.Value = value; }
        }

        public string Namespace
        {
            get { return _namespace.Value; }
            set { _namespace.Value = value; }
        }

        public string Description
        {
            get { return _description.Value; }
            set { _description.Value = value; }
        }

        public string FullName
        {
            get { return Name + "-" + SemanticVersion; }
        }

        public PackageIdentifier Identifier
        {
            get { return new PackageIdentifier(Name, SemanticVersion); }
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public ICollection<PackageNameOverride> Overrides
        {
            get { return _overrides; }
        }

        public bool UseProjectRepository
        {
            get { return _useProjectRepository.Value; }
            set { _useProjectRepository.Value = value; }
        }

        public bool UseSymLinks
        {
            get { return _useSymLinks.Value; }
            set { _useSymLinks.Value = value; }
        }

        [Obsolete("Please use the SemanticVersion property instead.")]
        public Version Version
        {
            get { return _version.Value; }
            set { _version.Value = value; }
        }

#pragma warning disable 612,618
        public SemanticVersion SemanticVersion
        {
            get { return _semanticVersion.Value ?? Version.ToSemVer(); }
            set { _semanticVersion.Value = value; }
        }
#pragma warning restore 612,618

        public string ReferencedAssemblies
        {
            get { return _referencedAssemblies.Value; }
            set { _referencedAssemblies.Value = value; }
        }

        public ICollection<string> DirectoryStructure
        {
            get { return _directoryStructure; }
        }

        public bool StorePackages
        {
            get { return _storePackages.Value; }
            set { _storePackages.Value = value; }
        }

        public IEnumerable<string> AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public IEnumerable<string> Authors
        {
            get { return _author; }
        }

        public string Copyright
        {
            get { return _copyright.Value; }
        }

        public string BuildConfiguration
        {
            get { return _buildConfiguration.Value; }
            set { _buildConfiguration.Value = value; }
        }

        public PackageDescriptorEntryCollection Entries
        {
            get { return _entries; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IPackageDescriptorEntry>)this).GetEnumerator();
        }

        IEnumerator<IPackageDescriptorEntry> IEnumerable<IPackageDescriptorEntry>.GetEnumerator()
        {
            foreach (IPackageDescriptorEntry line in Entries)
                yield return line;
        }

        void InitializeHeaders()
        {
            _dependencies = new PackageDependencyCollection(Entries);
            _overrides = new PackageNameOverrideCollection(Entries);
            _buildCommands = new MultiLine<string>(Entries, "build", _ => _, _ => _);
            _anchored = new SingleBoolValue(Entries, "anchored", false);
            _created = new SingleDateTimeOffsetValue(Entries, "created");
            _description = new SingleStringValue(Entries, "description");
            _name = new SingleStringValue(Entries, "name");
            _semanticVersion = new SingleSemanticVersionValue(Entries, "semantic-version");
            _version = new SingleVersionValue(Entries, "version");
            _useProjectRepository = new SingleBoolValue(Entries, "use-project-repository", true);
            _useSymLinks = new SingleBoolValue(Entries, "use-symlinks", false);
            _referencedAssemblies = new SingleStringValue(Entries, "referenced-assemblies", "*");
            _directoryStructure = new MultiLine<string>(Entries, "directory-structure", _ => _, _ => _);
            _title = new SingleStringValue(Entries, "title");
            _namespace = new SingleStringValue(Entries, "namespace");
            _storePackages = new SingleBoolValue(Entries, "store-packages", true);
            _assemblyInfo = new MultiLine<string>(Entries, "assembly-info", _ => _, _ => _);
            _author = new MultiLine<string>(Entries, "author", _ => _, _ => _);
            _maintainer = new MultiLine<string>(Entries, "maintainer", _ => _, _ => _);
            _copyright = new SingleStringValue(Entries, "copyright");
            _buildConfiguration = new SingleStringValue(Entries, "build-configuration");
            _trademark = new SingleStringValue(Entries, "trademark");
        }

        public IPackageDescriptor CreateScoped(IEnumerable<IPackageDescriptorEntry> scopedEntries)
        {
            return new ScopedPackageDescriptor(this, scopedEntries);
        }

        public IEnumerable<IPackageDescriptorEntry> GetPersistableEntries()
        {
            return Entries;
        }
    }
}