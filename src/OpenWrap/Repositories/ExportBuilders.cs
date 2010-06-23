using System.Collections.Generic;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public static class ExportBuilders
    {
        static readonly List<IExportBuilder> _exportBuilders = new List<IExportBuilder>
        {
            new AssemblyReferenceExportBuilder(),
            new CommandExportBuilder()
        };
        public static ICollection<IExportBuilder> All { get { return _exportBuilders; } }
    }
}