using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Resources;
using OpenRasta.Wrap.Sources;

namespace OpenRasta.Wrap.Repositories
{
    public class UncompressedPackage : IPackage
    {
        readonly FileInfo _originalWrapFile;
        readonly IEnumerable<IExportBuilder> _exporters;

        public UncompressedPackage(FileInfo originalPackage, string wrapCacheDirectory, IEnumerable<IExportBuilder> exporters)
        {
            _originalWrapFile = originalPackage;
            _exporters = exporters;
            BaseDirectory = new DirectoryInfo(wrapCacheDirectory);
            // get the descriptor file inside the package
            var descriptorName = BaseDirectory.Name;
            Descriptor = new WrapDescriptorParser().ParseFile(Path.Combine(wrapCacheDirectory, descriptorName + ".wrapdesc"));
        }

        protected DirectoryInfo BaseDirectory { get; set; }

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

        protected WrapDescriptor Descriptor { get; set; }

        public IWrapExport GetExport(string exportName, WrapRuntimeEnvironment environment)
        {
            // get the list of exports in the 
            var exporter =
                _exporters.SingleOrDefault(x => x.ExportName.Equals(exportName, StringComparison.OrdinalIgnoreCase));
            if (exporter == null)
                return null;

            var exports = from directory in BaseDirectory.GetDirectories()
                          where exporter.CanProcessExport(directory.Name)
                          let directoryPath = directory.FullName
                          select (IWrapExport)new FolderExport(directory)
                          {
                              Items = directory.GetFiles()
                                  .Select(x => (IWrapExportItem)new FileExportItem(x))
                                  .ToList()
                          };

            return exporter.ProcessExports(exports, environment);
        }

        public void Persist(string folder)
        {
            File.Copy(_originalWrapFile.FullName, Path.Combine(folder, _originalWrapFile.Name),true);
        }
    }
}