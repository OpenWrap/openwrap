using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public class DefaultPackageInfo : IPackageInfo
    {
        readonly PackageDescriptor _descriptor;
        readonly string _packageName;
        readonly Version _packageVersion;

        public PackageIdentifier Identifier { get; private set; }

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
            get { return _packageVersion; }
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

        public DateTimeOffset CreationTime
        {
            get { return _descriptor.CreationTime; }
        }

        public bool Anchored
        {
            get { return _descriptor.Anchored; }
            set { _descriptor.Anchored = value; }
        }

        public string Description
        {
            get { return _descriptor.Description; }
        }

        public bool Nuked { get { return false; } }

        public DefaultPackageInfo(string packageFileName, Version versionFileContent, PackageDescriptor descriptor)
        {
            _descriptor = descriptor;
            _packageVersion = versionFileContent 
                ?? descriptor.Version 
                ?? PackageNameUtility.GetVersion(packageFileName);
            _packageName = PackageNameUtility.GetName(packageFileName);
            Identifier = new PackageIdentifier(Name, Version);
        }
    }
}