using System;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using OpenRasta.Client;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuGet
{
    public class NuGetFeedNavigator : IHttpRepositoryNavigator
    {
        readonly Uri _feedUri;
        readonly IHttpClient _httpClient = new HttpWebRequestBasedClient();
        PackageDocument _packageDocument;

        public NuGetFeedNavigator(Uri feedUri)
        {
            _feedUri = feedUri;
        }

        public bool CanPublish
        {
            get { return false; }
        }

        public PackageDocument Index()
        {
            EnsureFeedLoaded();
            return _packageDocument;
        }

        public Stream LoadPackage(PackageItem packageItem)
        {
            var response = _httpClient.CreateRequest(packageItem.PackageHref).Get().Send();
            if (response.Entity == null)
                return null;
            var ms = new MemoryStream();
            NuGetConverter.Convert(response.Entity.Stream, ms);
            ms.Position = 0;
            return ms;
        }

        public void PushPackage(string packageFileName, Stream packageStream)
        {
            throw new NotSupportedException("Legacy NuGet feeds do not include upload support.");
        }

        void EnsureFeedLoaded()
        {
            if (_packageDocument == null)
                _packageDocument = SyndicationFeed.Load<NuGetSyndicationFeed>(XmlReader.Create(_feedUri.ToString())).ToPackageDocument();
        }
    }
}