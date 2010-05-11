using System.Reflection;
using OpenRasta.Wrap.Resources;

namespace OpenRasta.Wrap.Repositories
{
    public class AssemblyReferenceExportItem : IAssemblyReferenceExportItem
    {
        private readonly IWrapExportItem _wrapped;

        public AssemblyReferenceExportItem(IWrapExportItem item)
        {
            _wrapped = item;
            AssemblyName = AssemblyName.GetAssemblyName(item.FullPath);
        }

        public AssemblyName AssemblyName { get; set; }
        public string FullPath
        {
            get { return _wrapped.FullPath; }
        }
    }
}