using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement.Packages
{
    public class ZipPackage : IPackageInfo
    {
        readonly LazyValue<PackageIdentifier> _identifier;
        IPackageInfo _descriptor;

        protected ZipPackage(IFile packageFile)
        {
            PackageFile = packageFile;
            _identifier = new LazyValue<PackageIdentifier>(() => new PackageIdentifier(Name, Version));
        }

        public bool Anchored
        {
            get { return Descriptor.Anchored; }
        }

        public DateTimeOffset Created
        {
            get { return PackageFile.LastModifiedTimeUtc != null ? new DateTimeOffset(PackageFile.LastModifiedTimeUtc.Value) : DateTimeOffset.UtcNow; }
        }

        public ICollection<PackageDependency> Dependencies
        {
            get { return Descriptor.Dependencies; }
        }

        public string Description
        {
            get { return Descriptor.Description; }
        }

        public IPackageInfo Descriptor
        {
            get
            {
                if (_descriptor == null)
                    LoadDescriptor();
                return _descriptor;
            }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public PackageIdentifier Identifier
        {
            get { return _identifier; }
        }

        public string Name
        {
            get { return Descriptor.Name; }
        }

        public bool Nuked
        {
            get { return Descriptor.Nuked; }
        }

        public IFile PackageFile { get; set; }

        public IPackageRepository Source { get; set; }

        public Version Version
        {
            get { return Descriptor.Version; }
        }

        public virtual IPackage Load()
        {
            return null;
        }

        void LoadDescriptor()
        {
            using (Stream zipStream = PackageFile.OpenRead())
            using (var zip = new ZipFile(zipStream))
            {
                var entries = zip.Cast<ZipEntry>();
                ZipEntry descriptorFile = entries.FirstOrDefault(x => x.Name.EndsWith(".wrapdesc"));
                if (descriptorFile == null)
                    throw new InvalidOperationException(String.Format("The package '{0}' doesn't contain a valid .wrapdesc file.", PackageFile.Name));

                ZipEntry versionFile = entries.SingleOrDefault(x => x.Name.EqualsNoCase("version"));
                Version versionFromVersionFile = versionFile != null ? zip.Read(versionFile, x => StringExtensions.ToVersion(x.ReadString())) : null;
                PackageDescriptor descriptor = zip.Read(descriptorFile, x => new PackageDescriptorReaderWriter().Read(x));

                _descriptor = new DefaultPackageInfo(PackageFile.Name, versionFromVersionFile, descriptor);

                if (Descriptor.Version == null)
                    throw new InvalidOperationException("The package '{0}' doesn't have a valid version, looked in the 'wrapdesc' file, in 'version' and in the package file-name.");
            }
        }
    }
}