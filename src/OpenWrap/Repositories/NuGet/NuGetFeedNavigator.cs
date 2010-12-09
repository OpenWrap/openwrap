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

namespace OpenWrap.Repositories.NuGet
{
    public class NuGetFeedNavigator : IHttpRepositoryNavigator
    {
        Uri _feedUri;
        PackageDocument _packageDocument;
        readonly IHttpClient _httpClient = new HttpWebRequestBasedClient();

        public NuGetFeedNavigator(Uri feedUri)
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
                _packageDocument = SyndicationFeed.Load<NuGetSyndicationFeed>(XmlReader.Create(_feedUri.ToString())).ToPackageDocument();
        }

        public Stream LoadPackage(PackageItem packageItem)
        {
           var response =  _httpClient.CreateRequest(packageItem.PackageHref).Get().Send();
           if (response.Entity == null)
               return null;
            var ms = new MemoryStream();
            NuGetConverter.Convert(response.Entity.Stream, ms);
            ms.Position = 0;
            return ms;
        }

        public bool CanPublish
        {
            get { return false; }
        }

        public void PushPackage(string packageFileName, Stream packageStream)
        {
            throw new NotSupportedException("Legacy NuGet feeds do not include upload support.");
        }
    }
}
