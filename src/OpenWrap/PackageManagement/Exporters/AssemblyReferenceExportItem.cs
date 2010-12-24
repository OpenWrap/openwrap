using System.Reflection;

namespace OpenWrap.PackageManagement.Exporters
{
    public class AssemblyReferenceExportItem : IAssemblyReferenceExportItem
    {
        readonly IExportItem _wrapped;

        public AssemblyReferenceExportItem(IExportItem item)
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