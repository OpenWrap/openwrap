using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public interface IExportItem
    {
        string Path { get; }
        IPackage Package { get; }
    }
}