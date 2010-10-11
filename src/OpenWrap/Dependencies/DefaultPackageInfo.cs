using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public class DefaultPackageInfo : IPackageInfo
    {
        readonly WrapDescriptor _descriptor;
        readonly string _packageName;
        readonly Version _packageVersion;

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

        public DefaultPackageInfo(string packageFileName, Version versionFileContent, WrapDescriptor descriptor)
        {
            _descriptor = descriptor;
            _packageVersion = versionFileContent ?? PackageNameUtility.GetVersion(packageFileName);
            _packageName = PackageNameUtility.GetName(packageFileName);
        }
    }
}