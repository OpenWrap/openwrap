using System.IO;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories.Http
{
    public interface IHttpRepositoryNavigator
    {
        PackageFeed Index();
        Stream LoadPackage(PackageEntry packageEntry);
        bool CanPublish { get; }
        void PushPackage(string packageFileName, Stream packageStream);
    }
}