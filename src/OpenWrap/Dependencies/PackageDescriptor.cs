using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public class PackageDescriptor : IEnumerable<IPackageDescriptorLine>
    {
        readonly DescriptorLineCollection _lines = new DescriptorLineCollection();
        SingleBoolValue _anchored;
        SingleStringValue _buildCommand;
        SingleDateTimeOffsetValue _created;
        PackageDependencyCollection _dependencies;
        SingleStringValue _description;
        SingleStringValue _name;
        SingleBoolValue _useProjectRepository;
        SingleVersionValue _version;
        SingleBoolValue _useSymLinks;
        PackageNameOverrideCollection _overrides;

        public PackageDescriptor(IEnumerable<IPackageDescriptorLine> lines)
        {
            foreach (var line in lines)
                _lines.Append(line.Name, line.Value);
            InitializeHeaders();
        }

        public PackageDescriptor()
        {
            InitializeHeaders();
        }

        void InitializeHeaders()
        {
            _dependencies = new PackageDependencyCollection(_lines);
            _overrides = new PackageNameOverrideCollection(_lines);

            _anchored = new SingleBoolValue(_lines, "anchored", false);
            _buildCommand = new SingleStringValue(_lines, "build");
            _created = new SingleDateTimeOffsetValue(_lines, "created");
            _description = new SingleStringValue(_lines, "description");
            _name = new SingleStringValue(_lines, "name");
            _version = new SingleVersionValue(_lines, "version");
            _useProjectRepository = new SingleBoolValue(_lines, "use-project-repository", true);
            _useSymLinks = new SingleBoolValue(_lines, "use-symlinks", true);
        }

        public bool Anchored
        {
            get { return _anchored.Value; }
            set { _anchored.Value = value; }
        }

        public string BuildCommand
        {
            get { return _buildCommand.Value; }
            set { _buildCommand.Value = value; }
        }

        public DateTimeOffset Created
        {
            get { return _created.Value; }
            private set { _created.Value = value; }
        }

        public ICollection<PackageDependency> Dependencies { get { return _dependencies; } }

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

        public bool Nuked
        {
            get { return false; }
        }

        public ICollection<PackageNameOverride> Overrides { get { return _overrides; } }

        public IPackageRepository Source
        {
            get { return null; }
        }

        public bool UseProjectRepository
        {
            get { return _useProjectRepository.Value; }
            set { _useProjectRepository.Value = value; }
        }

        public Version Version
        {
            get { return _version.Value; }
            set { _version.Value = value; }
        }

        public bool UseSymLinks
        {
            get { return _useSymLinks.Value; }
            set { _useSymLinks.Value = value; }
        }

        public IPackage Load()
        {
            return null;
        }

        public IEnumerator<IPackageDescriptorLine> GetEnumerator()
        {
            foreach(var line in _lines)
                yield return line;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}