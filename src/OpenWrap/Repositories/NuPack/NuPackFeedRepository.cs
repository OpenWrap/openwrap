using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using OpenFileSystem.IO;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuPack
{
    public class NuPackFeedRepository
    {
    }
    public class NuPackFeedNavigator : IHttpRepositoryNavigator
    {
        Uri _feedUri;
        PackageDocument _packageDocument;

        public NuPackFeedNavigator(Uri feedUri)
        {
            _feedUri = feedUri;
        }
        public PackageDocument Index()
        {
            EnsureFeedLoaded();
            return _packageDocument;
        }

        void EnsureFeedLoaded()
        {
            if (_packageDocument == null)
                _packageDocument = SyndicationFeed.Load<NuPackSyndicationFeed>(XmlReader.Create(_feedUri.ToString())).ToPackageDocument();
        }

        public Stream LoadPackage(PackageItem packageItem)
        {
            // need to downlaod and up-convert the package.
            throw new NotImplementedException();
        }

        public bool CanPublish
        {
            get { return false; }
        }

        public void PushPackage(string packageFileName, Stream packageStream)
        {
            throw new NotSupportedException("Legacy NuPack feeds do not include upload support.");
        }
    }
}
