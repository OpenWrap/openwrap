using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuPack
{
    public class NuPackFeedNavigator : IHttpRepositoryNavigator
    {
        Uri _feedUri;
        PackageDocument _packageDocument;
        readonly IHttpClient _httpClient = new HttpWebRequestBasedClient();

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
           var response =  _httpClient.CreateRequest(packageItem.PackageHref).Get().Send();
           if (response.Entity == null)
               return null;
            var ms = new MemoryStream();
            NuPackConverter.Convert(response.Entity.Stream, ms);
            ms.Position = 0;
            return ms;
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
