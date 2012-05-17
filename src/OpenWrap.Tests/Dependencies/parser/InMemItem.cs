using OpenWrap.PackageManagement;
using OpenWrap.PackageModel;

namespace Tests.Dependencies.parser
{
    public class InMemItem : IExportItem
    {
        public string Path { get; set; }

        public IPackage Package { get; set; }
    }
}