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
        IPackageInfo _descriptor;
        bool? _isValid;


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
            get { return PackageFile.LastModifiedTimeUtc != null ? PackageFile.LastModifiedTimeUtc.Value : DateTimeOffset.UtcNow; }
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
            get { return Descriptor.Nuked; }
        }

        public bool IsValid
        {
            get
            {
                if (_isValid == null)
                {
                    using(var stream = PackageFile.OpenRead())
                    using (var zip = new ZipFile(stream))
                    {
                        try
                        {
                            var entries = zip.Cast<ZipEntry>().ToList();
                            _isValid =  entries.Count > 1 && entries.Any(x => x.Name.EndsWithNoCase(".wrapdesc"));

                        }catch
                        {
                            _isValid = false;
                        }
                    }
                }
                return (bool)_isValid;
            }
        }

        public IFile PackageFile { get; set; }

        public IPackageRepository Source { get; protected set; }

        public SemanticVersion SemanticVersion
        {
            get { return Descriptor.SemanticVersion; }
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
                    throw new InvalidOperationException(String.Format("The package '{0}' doesn't contain a valid .wrapdesc file.", PackageFile.Name));

                ZipEntry versionFile = entries.SingleOrDefault(x => x.Name.EqualsNoCase("version"));
                SemanticVersion versionFromVersionFile = versionFile != null ? zip.Read(versionFile, x => x.ReadString().ToSemVer()) : null;
                var descriptor = zip.Read(descriptorFile, x => new PackageDescriptorReader().Read(x));

                _descriptor = new DefaultPackageInfo(versionFromVersionFile, descriptor);

                if (Descriptor.SemanticVersion == null)
                    throw new InvalidOperationException(string.Format("The package '{0}' doesn't have a valid version, looked in the 'wrapdesc' file, in 'version' and in the package file-name.", descriptor.Name));
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