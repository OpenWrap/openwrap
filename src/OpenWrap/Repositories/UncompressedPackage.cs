using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Tasks;

namespace OpenWrap.Repositories
{
    public class UncompressedPackage : IPackage
    {
        readonly IFile _originalWrapFile;
        readonly IEnumerable<IExportBuilder> _exporters;
        static readonly TraceSource _log = new TraceSource("openwrap", SourceLevels.All);
        Version _version;

        public UncompressedPackage(IPackageRepository source,
                                   IFile originalPackage,
                        IDirectory wrapCacheDirectory,
            IEnumerable<IExportBuilder> exporters)
        {
            _originalWrapFile = originalPackage;
            _exporters = exporters;
            BaseDirectory = wrapCacheDirectory;
            // get the descriptor file inside the package
            var descriptorName = originalPackage.NameWithoutExtension;
            Source = source;
            var wrapDescriptor = wrapCacheDirectory.Files("*.wrapdesc").SingleOrDefault();
            if (wrapDescriptor == null)
                throw new InvalidOperationException("Could not find descriptor in wrap cache directory, or there are multiple .wrapdesc files in the package.");
            var versionFile = wrapCacheDirectory.GetFile("version");
            Descriptor = new DefaultPackageInfo(originalPackage.Name, versionFile.Exists ? versionFile.Read(x=>x.ReadString().ToVersion()) : null, new WrapDescriptorParser().ParseFile(wrapDescriptor));
        }

        protected IDirectory BaseDirectory { get; set; }

        public ICollection<PackageDependency> Dependencies
        {
            get { return Descriptor.Dependencies; }
        }
        public string Description { get { return Descriptor.Description; } }
        public bool Anchored { get { return Descriptor.Anchored; } }

        public string Name
        {
            get { return Descriptor.Name; }
        }

        public Version Version
        {
            get { return Descriptor.Version ?? _version; }
        }

        public IPackage Load()
        {
            return this;
        }

        public IPackageRepository Source
        {
            get;
            private set;
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTimeOffset CreationTime
        {
            get { if (_originalWrapFile.LastModifiedTimeUtc != null) return new DateTimeOffset(_originalWrapFile.LastModifiedTimeUtc.Value);
                return DateTimeOffset.UtcNow;
            }
        }

        protected DefaultPackageInfo Descriptor { get; set; }

        public bool Nuked { get { return false; } }

        public IExport GetExport(string exportName, ExecutionEnvironment environment)
        {
            var exporter =
                _exporters.FirstOrDefault(x => x.ExportName.Equals(exportName, StringComparison.OrdinalIgnoreCase));

            var directories = BaseDirectory.Directories();
            if (exporter != null)
                directories = directories.Where(x => exporter.CanProcessExport(x.Name));
            else
                directories = directories.Take(1);
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
            return _originalWrapFile.OpenRead();
        }
    }
}