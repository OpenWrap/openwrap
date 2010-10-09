using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuPack
{
    public class NuPackFeedRepository
    {
    }
    public class NuPackFeedNavigator : IHttpRepositoryNavigator
    {
        public NuPackFeedNavigator(Stream feedStream)
        {
            
        }
        public PackageDocument Index()
        {
            throw new NotImplementedException();
        }

        public Stream LoadPackage(PackageItem packageItem)
        {
            throw new NotImplementedException();
        }

        public bool CanPublish
        {
            get { throw new NotImplementedException(); }
        }

        public void PushPackage(string packageFileName, Stream packageStream)
        {
            throw new NotImplementedException();
        }
    }
}
