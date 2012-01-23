using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.PackageModel
{
    
    public class DefaultPackageInfo : IPackageInfo
    {
        readonly IPackageDescriptor _descriptor;
        //readonly string _packageName;
        readonly SemanticVersion _packageSemanticVersion;

        public DefaultPackageInfo(SemanticVersion versionFileContent, IPackageDescriptor descriptor)
        {
            _descriptor = descriptor;
            _packageSemanticVersion = versionFileContent
                              ?? descriptor.SemanticVersion;
            //_packageName = PackageNameUtility.GetName(packageFileName);
            Identifier = new PackageIdentifier(Name, SemanticVersion);
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

        [Obsolete("Plase use SemanticVersion")]
        public Version Version
        {
            get { return SemanticVersion != null ? SemanticVersion.ToVersion() : null; }
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
            get { return _descriptor.Name; }
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

        public SemanticVersion SemanticVersion
        {
            get { return _packageSemanticVersion; }
        }


        public IPackage Load()
        {
            return null;
        }
    }
}