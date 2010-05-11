using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Resources;
using OpenRasta.Wrap.Sources;

namespace OpenRasta.Wrap.Repositories
{
    public class FolderWrapPackage : IWrapPackage
    {
        readonly IEnumerable<IExportBuilder> _exporters;

        public FolderWrapPackage(string folderPath, IEnumerable<IExportBuilder> exporters)
        {
            _exporters = exporters;
            BaseDirectory = new DirectoryInfo(folderPath);
            // get the descriptor file inside the package
            var descriptorName = BaseDirectory.Name;
            Descriptor = new WrapDescriptorParser().Parse(Path.Combine(folderPath, descriptorName + ".wrapdesc"));
        }

        protected DirectoryInfo BaseDirectory { get; set; }

        public ICollection<WrapDependency> Dependencies
        {
            get { return Descriptor.Dependencies; }
        }

        public string FolderPath { get; set; }

        public string Name
        {
            get { return Descriptor.Name; }
        }

        public Version Version
        {
            get { return Descriptor.Version; }
        }

        protected WrapDescriptor Descriptor { get; set; }

        public IWrapExport GetExport(string exportName, WrapRuntimeEnvironment environment)
        {
            // get the list of exports in the 
            var exporter =
                _exporters.SingleOrDefault(
                    x => string.Compare(x.ExportName, exportName, StringComparison.OrdinalIgnoreCase) == 0);
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
    }
}