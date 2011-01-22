using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Repositories;

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
        SingleVersionValue _version;
        SingleStringValue _referencedAssemblies;
        MultiLine<string> _buildCommands;

        public PackageDescriptor(IEnumerable<IPackageDescriptorEntry> entries)
        {
            foreach (var line in entries)
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
        public ICollection<string> Build { get { return _buildCommands; } }
        public ICollection<PackageDependency> Dependencies
        {
            get { return _dependencies; }
        }

        public string Description
        {
            get { return _description.Value; }
            set { _description.Value = value; }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public PackageIdentifier Identifier
        {
            get { return new PackageIdentifier(Name, Version); }
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

        public Version Version
        {
            get { return _version.Value; }
            set { _version.Value = value; }
        }

        public string ReferencedAssemblies
        {
            get { return _referencedAssemblies.Value; }
            set { _referencedAssemblies.Value = value; }
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
            foreach (var line in Entries)
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
            _version = new SingleVersionValue(Entries, "version");
            _useProjectRepository = new SingleBoolValue(Entries, "use-project-repository", true);
            _useSymLinks = new SingleBoolValue(Entries, "use-symlinks", true);
            _referencedAssemblies = new SingleStringValue(Entries, "referenced-assemblies", "*");
        }
        public IPackageDescriptor CreateScoped(IEnumerable<IPackageDescriptorEntry> scopedEntries)
        {
            return new ScopedPackageDescriptor(this, scopedEntries);
        }
    }
}