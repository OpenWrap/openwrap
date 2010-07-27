using System.IO;

namespace OpenWrap.Repositories.Http
{
    public interface IHttpRepositoryNavigator
    {
        PackageDocument Index();
        Stream LoadPackage(PackageItem packageItem);
    }
}