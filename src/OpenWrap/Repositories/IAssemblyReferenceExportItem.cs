using System.Reflection;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Repositories
{
    public interface IAssemblyReferenceExportItem : IWrapExportItem
    {
        AssemblyName AssemblyName { get; set; }
    }
}