using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters.Assemblies
{
    public class DefaultAssemblyExporter : AbstractAssemblyExporter
    {
        public DefaultAssemblyExporter(string exportName = "bin")
                : base(exportName)
        {
        }
    }
}