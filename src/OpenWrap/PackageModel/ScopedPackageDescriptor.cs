using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel.Parsers;

namespace OpenWrap.PackageModel
{
    public partial class PackageDescriptor
    {
        sealed class ScopedPackageDescriptor : IPackageDescriptor
        {
            readonly PackageDescriptorEntryCollection _entries = new PackageDescriptorEntryCollection();

            DelegatedValue<bool> _anchored;
            DelegatedValue<DateTimeOffset> _created;
            DelegatedValue<string> _description;
            DelegatedValue<string> _name;
            DelegatedValue<bool> _useProjectRepository;
            DelegatedValue<bool> _useSymLinks;
            DelegatedValue<Version> _version;
            DelegatedValue<string> _referencedAssemblies;

            ScopedPackageNameOverrideCollection _overrides;
            ScopedPackageDependencyCollection _dependencies;
            DelegatedMultiLine<string> _buildCommands;
            DelegatedMultiLine<string> _directoryStructures;
            readonly PackageDescriptor _parent;
            DelegatedValue<string> _title;

            public ScopedPackageDescriptor(PackageDescriptor parent, IEnumerable<IPackageDescriptorEntry> entries)
                : this(parent)
            {
                foreach (var line in entries)
                    _entries.Append(line);
                InitializeHeaders();
            }

            public ScopedPackageDescriptor(PackageDescriptor parent)
            {
                _parent = parent;
            }

            public bool Anchored
            {
                get { return _anchored.Value; }
                set { _anchored.Value = value; }
            }


            public DateTimeOffset Created
            {
                get { return _created.Value; }
                private set { _created.Value = value; }
            }
            public ICollection<string> Build { get { return _buildCommands; } }
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
                get { return _parent.Namespace; }
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

            public ICollection<string> DirectoryStructure
            {
                get { return _parent.DirectoryStructure; }
            }

            public bool StorePackages
            {
                get { return _parent.StorePackages; }
            }

            public IPackageDescriptor CreateScoped(IEnumerable<IPackageDescriptorEntry> read)
            {
                throw new InvalidOperationException("Can only have one level of nesting for scoped descriptors.");
            }

            public IEnumerable<IPackageDescriptorEntry> GetPersistableEntries()
            {
                foreach (var entry in _entries)
                    yield return entry;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<IPackageDescriptorEntry>)this).GetEnumerator();
            }

            IEnumerator<IPackageDescriptorEntry> IEnumerable<IPackageDescriptorEntry>.GetEnumerator()
            {
                foreach (var line in _parent.Entries.Where(_ => !_.Name.EqualsNoCase("depends") && !_.Name.EqualsNoCase("override")))
                    yield return line;
                foreach (var dependency in _dependencies)
                    yield return new GenericDescriptorEntry("depends", dependency.ToString());

                foreach (var packageOverride in _overrides)
                    yield return new GenericDescriptorEntry("override", string.Format("{0} {1}", packageOverride.OldPackage, packageOverride.NewPackage));
            }

            void InitializeHeaders()
            {
                _dependencies = new ScopedPackageDependencyCollection(_parent._dependencies, new PackageDependencyCollection(_entries));
                _overrides = new ScopedPackageNameOverrideCollection(_parent._overrides, new PackageNameOverrideCollection(_entries));
                _buildCommands = new DelegatedMultiLine<string>(
                    new MultiLine<string>(_parent.Entries, "build", _ => _, _ => _),
                    new MultiLine<string>(_entries, "build", _ => _, _ => _)
                    );

                _anchored = CreateDelegated("anchored", SingleBoolValue.New, false);


                _created = CreateDelegated<DateTimeOffset>("created", SingleDateTimeOffsetValue.New);
                _description = CreateDelegated<string>("description", SingleStringValue.New);

                _name = CreateDelegated<string>("name", SingleStringValue.New);
                _version = CreateDelegated<Version>("version", SingleVersionValue.New);
                _useProjectRepository = CreateDelegated("use-project-repository", SingleBoolValue.New, true);
                _useSymLinks = CreateDelegated("use-symlinks", SingleBoolValue.New, false);
                _referencedAssemblies = CreateDelegated("referenced-assemblies", SingleStringValue.New, "*");
                _title = CreateDelegated<string>("title", SingleStringValue.New);

                _directoryStructures = new DelegatedMultiLine<string>(
                    new MultiLine<string>(_parent.Entries, "directory-structure", _=>_,_=>_),
                    new MultiLine<string>(_entries, "directory-structure", _=>_,_=>_)
                    );
            }

            DelegatedValue<T> CreateDelegated<T>(string name, Func<PackageDescriptorEntryCollection, string, T, SingleValue<T>> ctor, T defaultValue = default(T))
            {
                return new DelegatedValue<T>(
                        ctor(_parent.Entries, name, defaultValue),
                        ctor(_entries, name, defaultValue));
            }
        }
    }
}