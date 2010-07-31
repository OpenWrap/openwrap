using System.IO;

namespace OpenWrap.Repositories.Http
{
    public interface IHttpRepositoryNavigator
    {
        PackageDocument Index();
        Stream LoadPackage(PackageItem packageItem);
        bool CanPublish { get; }
        void PushPackage(string packageFileName, Stream packageStream);
    }
}