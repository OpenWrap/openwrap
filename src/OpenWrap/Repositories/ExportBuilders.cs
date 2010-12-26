using System.Collections.Generic;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;

namespace OpenWrap.Repositories
{
    // TODO: Refactor and take away, quick
    public static class ExportBuilders
    {
        static readonly List<IExportBuilder> _exportBuilders = new List<IExportBuilder>
        {
                new AssemblyReferenceExportBuilder(),
                new CommandExportBuilder()
        };

        public static ICollection<IExportBuilder> All
        {
            get { return _exportBuilders; }
        }
    }
}