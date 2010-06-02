using System.Reflection;

namespace OpenWrap.Exports
{
    public interface IAssemblyReferenceExportItem : IExportItem
    {
        AssemblyName AssemblyName { get; set; }
    }
}