using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public class WrapDescriptor : IPackageInfo
    {
        public WrapDescriptor()
        {
            Dependencies = new List<PackageDependency>();
            Overrides = new List<PackageNameOverride>();
            Description = "";
        }

        public ICollection<PackageDependency> Dependencies { get; set; }
        public ICollection<PackageNameOverride> Overrides { get; set; }

        public string Name { get; set; }

        public Version Version { get; set; }
        public bool IsVersionInDescriptor { get; set; }
        public IFile File { get; set; }
        public string Description { get; set; }

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

        public DateTime? LastModifiedTimeUtc
        {
            get { return null; }
        }

        public bool Anchored { get; set; }

        public string BuildCommand { get; set; }

        public bool IsCompatibleWith(Version version)
        {
            return false;
        }
    }
    public class DefaultPackageInfo : IPackageInfo
    {
        WrapDescriptor _descriptor;
        string _packageName;
        Version _packageVersion;

        public ICollection<PackageDependency> Dependencies
        {
            get { return _descriptor.Dependencies; }
        }

        
        public string Name
        {
            get { return _descriptor.Name ?? _packageName; }
            set { _descriptor.Name = value; }
        }

        public Version Version
        {
            get { return _descriptor.Version ?? _packageVersion; }
            set { _descriptor.Version = value; }
        }


        public IPackage Load()
        {
            return _descriptor.Load();
        }

        public IPackageRepository Source
        {
            get { return _descriptor.Source; }
        }

        public string FullName
        {
            get { return _descriptor.FullName; }
        }

        public DateTime? LastModifiedTimeUtc
        {
            get { return _descriptor.LastModifiedTimeUtc; }
        }

        public bool Anchored
        {
            get { return _descriptor.Anchored; }
            set { _descriptor.Anchored = value; }
        }

        public DefaultPackageInfo(string packageFileName, Version versionFileContent, WrapDescriptor descriptor)
        {
            _descriptor = descriptor;
            _packageVersion = versionFileContent ?? PackageNameUtility.GetVersion(packageFileName);
            _packageName = PackageNameUtility.GetName(packageFileName);
        }
    }
}