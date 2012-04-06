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
using StreamExtensions = OpenWrap.IO.StreamExtensions;

namespace OpenWrap.PackageManagement.Packages
{
    // TODO: Move to Stream-based API
    public class ZipFilePackage : IPackageInfo
    {
        readonly LazyValue<PackageIdentifier> _identifier;
        IPackageDescriptor _descriptor;
        SemanticVersion _semver;


        public ZipFilePackage(IFile packageFile)
        {
            PackageFile = packageFile;
            _identifier = new LazyValue<PackageIdentifier>(() => new PackageIdentifier(Name, SemanticVersion));
            Source = new InMemoryRepository("Null repository.") { Packages = { this } };
        }

        public bool Anchored
        {
            get { return Descriptor.Anchored; }
        }

        public DateTimeOffset Created
        {
            get
            { 
                return Descriptor.Created != default(DateTimeOffset)
                ? Descriptor.Created
                : PackageFile.LastModifiedTimeUtc != null ? PackageFile.LastModifiedTimeUtc.Value : default(DateTimeOffset);
            }
        }

        [Obsolete("Plase use SemanticVersion")]
        public Version Version
        {
            get { return SemanticVersion != null ? SemanticVersion.ToVersion() : null; }
        }
        public ICollection<PackageDependency> Dependencies
        {
            get { return Descriptor.Dependencies; }
        }
        public string Title { get { return Descriptor.Title; } }

        public string Description
        {
            get { return Descriptor.Description; }
        }

        public IPackageDescriptor Descriptor
        {
            get
            {
                EnsureDescriptorLoaded();
                return _descriptor;
            }
        }

        void EnsureDescriptorLoaded()
        {
            if (_descriptor == null)
                LoadDescriptor();
        }

        public string FullName
        {
            get { return Name + "-" + SemanticVersion; }
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
            get { return false; }
        }

        public bool IsValid
        {
            get { return Descriptor != null && Name != null && _semver != null; }
        }


        public IFile PackageFile { get; set; }

        public IPackageRepository Source { get; protected set; }

        public SemanticVersion SemanticVersion
        {
            get
            {
                EnsureDescriptorLoaded();
                return _semver; 
            }
        }

        public virtual IPackage Load()
        {
            var tempDirectory = PackageFile.FileSystem.CreateTempDirectory();
            ExtractPackage(PackageFile, tempDirectory);
            return new UncompressedPackage(this.Source, PackageFile, tempDirectory);
        }

        void LoadDescriptor()
        {
            using (Stream zipStream = PackageFile.OpenRead())
            using (var zip = new ZipFile(zipStream))
            {
                var entries = zip.Cast<ZipEntry>();
                ZipEntry descriptorFile = entries.FirstOrDefault(x => x.Name.EndsWithNoCase(".wrapdesc"));
                if (descriptorFile == null)
                    return;

                ZipEntry versionFile = entries.SingleOrDefault(x => x.Name.EqualsNoCase("version"));
                SemanticVersion versionFromVersionFile = versionFile != null ? zip.Read(versionFile, x => x.ReadString().Replace("\r","").Replace("\n", "").ToSemVer()) : null;
                _descriptor = zip.Read(descriptorFile, x => new PackageDescriptorReader().Read(x));
                _semver = _descriptor.SemanticVersion ?? versionFromVersionFile ?? _descriptor.Version.ToSemVer();

            }
        }

        protected static bool ExtractPackage(IFile wrapFile, IDirectory destinationDirectory)
        {

            try
            {
                var nt = new WindowsNameTransform(destinationDirectory.Path.FullPath);
                using (var zipFile = new ZipFile(wrapFile.OpenRead()))
                {
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        if (zipEntry.IsFile)
                        {
                            IFile destinationFile;

                            if (System.IO.Path.DirectorySeparatorChar == '\\')
                                destinationFile = destinationDirectory.FileSystem.GetFile(nt.TransformFile(zipEntry.Name));
                            else
                                destinationFile = destinationDirectory.GetFile(zipEntry.Name);

                            using (var targetFile = destinationFile.MustExist().OpenWrite())
                            using (var sourceFile = zipFile.GetInputStream(zipEntry))
                                StreamExtensions.CopyTo(sourceFile, targetFile);

                            destinationFile.LastModifiedTimeUtc = zipEntry.DateTime.ToUniversalTime();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}