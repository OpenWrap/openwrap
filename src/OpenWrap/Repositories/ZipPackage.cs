using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class ZipPackage : IPackageInfo
    {
        public IFile PackageFile { get; set; }

        protected ZipPackage(IFile packageFile)
        {
            PackageFile = packageFile;
        }

        public bool Anchored
        {
            get { return Descriptor.Anchored; }
        }

        public bool Nuked
        {
            get { return Descriptor.Nuked; }
        }

        public DateTimeOffset CreationTime
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

        IPackageInfo _descriptor;
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

        public string Name
        {
            get { return Descriptor.Name; }
        }

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
                IEnumerable<ZipEntry> entries = zip.Cast<ZipEntry>();
                ZipEntry descriptorFile = entries.FirstOrDefault(x => x.Name.EndsWith(".wrapdesc"));
                if (descriptorFile == null)
                    throw new InvalidOperationException(String.Format("The package '{0}' doesn't contain a valid .wrapdesc file.", PackageFile.Name));

                ZipEntry versionFile = entries.SingleOrDefault(x => x.Name.Equals("version", StringComparison.OrdinalIgnoreCase));
                Version versionFromVersionFile = versionFile != null ? zip.Read(versionFile, x => x.ReadString().ToVersion()) : null;
                PackageDescriptor descriptor = zip.Read(descriptorFile, x => new PackageDescriptorReaderWriter().Read(x));

                _descriptor = new DefaultPackageInfo(PackageFile.Name, versionFromVersionFile, descriptor);

                if (Descriptor.Version == null)
                    throw new InvalidOperationException("The package '{0}' doesn't have a valid version, looked in the 'wrapdesc' file, in 'version' and in the package file-name.");
            }
        }
    }
}