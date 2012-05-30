using System.IO;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.FileSystem
{
    public interface IHttpRepositoryNavigator
    {
        PackageFeed Index();
        Stream LoadPackage(PackageEntry packageEntry);
        bool CanPublish { get; }
        void PushPackage(string packageFileName, Stream packageStream);
    }
}