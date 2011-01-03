using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            {
                var feed = LoadFeed(_feedUri.ToString());
                var packages = new List<PackageItem>(feed.ToPackageDocument().Packages);
                
                SyndicationLink nextLink;
                while ((nextLink = feed.Links.SingleOrDefault(x => x.RelationshipType.EqualsNoCase("next"))) != null)
                {
                    feed = LoadFeed(nextLink.GetAbsoluteUri().ToString());
                    packages.AddRange(feed.ToPackageDocument().Packages);
                }
                _packageDocument = new PackageDocument() { Packages = packages.AsReadOnly() };
            }
        }

        NuGetSyndicationFeed LoadFeed(string feedUri)
        {
            using(var xmlReader = XmlReader.Create(feedUri))
                return SyndicationFeed.Load<NuGetSyndicationFeed>(xmlReader);
        }
    }
}