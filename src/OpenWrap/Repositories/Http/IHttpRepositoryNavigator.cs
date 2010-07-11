using System;
using System.IO;
using System.Xml.Linq;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories
{
    public interface IHttpRepositoryNavigator
    {
        PackageDocument Index();
        Stream LoadPackage(PackageItem packageItem);
    }
}