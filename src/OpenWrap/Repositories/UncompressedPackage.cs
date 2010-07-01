using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    public class UncompressedPackage : IPackage
    {
        readonly IFile _originalWrapFile;
        readonly IEnumerable<IExportBuilder> _exporters;

        public UncompressedPackage(IPackageRepository source, IFile originalPackage, IDirectory wrapCacheDirectory, IEnumerable<IExportBuilder> exporters)
        {
            _originalWrapFile = originalPackage;
            _exporters = exporters;
            BaseDirectory = wrapCacheDirectory;
            // get the descriptor file inside the package
            var descriptorName = BaseDirectory.Name;
            Source = source;
            Descriptor = new WrapDescriptorParser().ParseFile(wrapCacheDirectory.GetFile(descriptorName + ".wrapdesc"));
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
            get; private set;
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
            // get the list of exports in the 
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