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

        public UncompressedPackage(IPackageRepository source,
                                   IFile originalPackage,
                        IDirectory wrapCacheDirectory,
            IEnumerable<IExportBuilder> exporters,
            bool allowAnchoring)
        {
            _originalWrapFile = originalPackage;
            _exporters = exporters;
            BaseDirectory = wrapCacheDirectory;
            // get the descriptor file inside the package
            var descriptorName = originalPackage.NameWithoutExtension;
            Source = source;
            var wrapDescriptor = wrapCacheDirectory.FindFile(descriptorName + ".wrapdesc")
                ?? wrapCacheDirectory.FindFile(WrapNameUtility.GetName(descriptorName) + ".wrapdesc");
            if (wrapDescriptor == null)
                throw new InvalidOperationException("Could not find descriptor in wrap cache directory");
            Descriptor = new WrapDescriptorParser().ParseFile(wrapDescriptor);
            if (allowAnchoring)
                VerifyAnchoring();
        }
        void VerifyAnchoring()
        {
            if (Descriptor.IsAnchored)
            {
                var anchoredDirectory = BaseDirectory.Parent // cache folder
                            .Parent // wraps folder
                            .GetDirectory(Descriptor.Name);
                if (anchoredDirectory.Exists)
                {
                    if (anchoredDirectory.IsHardLink && anchoredDirectory.Target.Equals(BaseDirectory))
                        return;
                    try
                    {
                        anchoredDirectory.Delete();
                    }
                    catch (Exception e)
                    {
                        throw new PackageException("The package '{0}' could not be anchored.", e);
                    }
                }
                var anchoredPath = anchoredDirectory.Path;
                BaseDirectory.LinkTo(anchoredPath.FullPath);
            }
        }

        protected IDirectory BaseDirectory { get; set; }

        public ICollection<WrapDependency> Dependencies
        {
            get { return Descriptor.Dependencies; }
        }

        public string Name
        {
            get { return Descriptor.Name; }
        }

        public Version Version
        {
            get { return Descriptor.Version; }
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

        public DateTime? LastModifiedTimeUtc
        {
            get { return _originalWrapFile.LastModifiedTimeUtc; }
        }

        protected WrapDescriptor Descriptor { get; set; }

        public IExport GetExport(string exportName, ExecutionEnvironment environment)
        {
            var exporter =
                _exporters.SingleOrDefault(x => x.ExportName.Equals(exportName, StringComparison.OrdinalIgnoreCase));
            if (exporter == null)
                return null;

            var exports = from directory in BaseDirectory.Directories()
                          where exporter.CanProcessExport(directory.Name)
                          select (IExport)new FolderExport(directory.Name)
                          {
                              Items = directory.Files()
                                  .Select(x => (IExportItem)new FileExportItem(x))
                                  .ToList()
                          };

            return exporter.ProcessExports(exports, environment);
        }

        public Stream OpenStream()
        {
            return _originalWrapFile.OpenRead();
        }
    }
}