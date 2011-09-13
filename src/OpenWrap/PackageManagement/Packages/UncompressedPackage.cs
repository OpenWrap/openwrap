using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Packages
{
    public class UncompressedPackage : IPackage
    {
        static readonly TraceSource _log = new TraceSource("openwrap", SourceLevels.All);
        readonly IEnumerable<IExportBuilder> _exporters;
        readonly IFile _originalPackageFile;
        Version _version;
        PackageDescriptor _descriptor;

        public UncompressedPackage(IPackageRepository source,
                                   IFile originalPackageFile,
                                   IDirectory wrapCacheDirectory,
                                   IEnumerable<IExportBuilder> exporters)
        {
            _originalPackageFile = originalPackageFile;
            _exporters = exporters;
            BaseDirectory = wrapCacheDirectory;
            // get the descriptor file inside the package
            var descriptorName = originalPackageFile.NameWithoutExtension;
            Source = source;
            var wrapDescriptor = wrapCacheDirectory.Files("*.wrapdesc").SingleOrDefault();
            if (wrapDescriptor == null)
                throw new InvalidOperationException("Could not find descriptor in wrap cache directory, or there are multiple .wrapdesc files in the package.");
            var versionFile = wrapCacheDirectory.GetFile("version");
            _descriptor = new PackageDescriptorReaderWriter().Read(wrapDescriptor);
            PackageInfo = new DefaultPackageInfo(originalPackageFile.Name,
                                                versionFile.Exists ? versionFile.Read(x => x.ReadString().ToVersion()) : null,
                                                _descriptor);
            Identifier = new PackageIdentifier(Name, Version);
        }

        public bool Anchored
        {
            get { return PackageInfo.Anchored; }
        }

        public DateTimeOffset Created
        {
            get
            {
                return _originalPackageFile.LastModifiedTimeUtc ?? DateTimeOffset.UtcNow;
            }
        }

        public ICollection<PackageDependency> Dependencies
        {
            get { return PackageInfo.Dependencies; }
        }

        public string Description
        {
            get { return PackageInfo.Description; }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public PackageIdentifier Identifier { get; private set; }

        public string Name
        {
            get { return PackageInfo.Name; }
        }

        public bool Nuked
        {
            get { return false; }
        }

        public IPackageRepository Source { get; private set; }

        public Version Version
        {
            get { return PackageInfo.Version ?? _version; }
        }

        protected IDirectory BaseDirectory { get; set; }

        protected DefaultPackageInfo PackageInfo { get; set; }

        public PackageDescriptor Descriptor
        {
            get { return _descriptor; }
        }

        public IExport GetExport(string exportName, ExecutionEnvironment environment)
        {
            var exporter =
                    _exporters.FirstOrDefault(x => x.ExportName.EqualsNoCase(exportName));

            var directories = BaseDirectory.Directories();
            if (exporter != null)
                directories = directories.Where(x => exporter.CanProcessExport(x.Name));
            else
                directories = directories.Where(x => x.Name.EqualsNoCase(exportName));
            var exports = from directory in directories
                          select (IExport)new FolderExport(directory.Name)
                          {
                                  Items = directory.Files()
                                          .Select(x => (IExportItem)new FileExportItem(x))
                                          .ToList()
                          };

            return exporter != null ? exporter.ProcessExports(exports, environment) : exports.FirstOrDefault();
        }

        public Stream OpenStream()
        {
            return _originalPackageFile.OpenRead();
        }

        public IPackage Load()
        {
            return this;
        }
    }
}