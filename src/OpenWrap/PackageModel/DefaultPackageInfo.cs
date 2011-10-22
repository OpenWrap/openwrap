using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.PackageModel
{
    public class DefaultPackageInfo : IPackageInfo
    {
        readonly IPackageDescriptor _descriptor;
        readonly string _packageName;
        readonly Version _packageVersion;

        public DefaultPackageInfo(string packageFileName, Version versionFileContent, IPackageDescriptor descriptor)
        {
            _descriptor = descriptor;
            _packageVersion = versionFileContent
                              ?? descriptor.Version
                              ?? PackageNameUtility.GetVersion(packageFileName);
            _packageName = PackageNameUtility.GetName(packageFileName);
            Identifier = new PackageIdentifier(Name, Version);
        }

        public bool Anchored
        {
            get { return _descriptor.Anchored; }
            set { _descriptor.Anchored = value; }
        }

        public DateTimeOffset Created
        {
            get { return _descriptor.Created; }
        }

        public ICollection<PackageDependency> Dependencies
        {
            get { return _descriptor.Dependencies; }
        }

        public string Description
        {
            get { return _descriptor.Description; }
        }

        public string FullName
        {
            get { return _descriptor.FullName; }
        }

        public PackageIdentifier Identifier { get; private set; }


        public string Name
        {
            get { return _descriptor.Name ?? _packageName; }
            set { _descriptor.Name = value; }
        }
        public string Title
        {
            get { return _descriptor.Title;  }
            set { _descriptor.Title = value; }
        }
        public bool Nuked
        {
            get { return false; }
        }

        public bool IsValid
        {
            get { return true; }
        }

        public IPackageRepository Source
        {
            get { return null; }
        }

        public Version Version
        {
            get { return _packageVersion; }
        }


        public IPackage Load()
        {
            return null;
        }
    }
}