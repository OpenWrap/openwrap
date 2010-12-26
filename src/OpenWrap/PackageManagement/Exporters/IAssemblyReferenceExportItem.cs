using System.Reflection;

namespace OpenWrap.PackageManagement.Exporters
{
    public interface IAssemblyReferenceExportItem : IExportItem
    {
        AssemblyName AssemblyName { get; set; }
    }
}