using System.Reflection;

namespace OpenWrap.Exports
{
    public class AssemblyReferenceExportItem : IAssemblyReferenceExportItem
    {
        private readonly IExportItem _wrapped;

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