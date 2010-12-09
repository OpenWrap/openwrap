using System.Linq;
using System.ServiceModel.Syndication;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuGet
{
    public class NuGetSyndicationFeed : SyndicationFeed
    {
        protected override SyndicationItem CreateItem()
        {
            return new NuGetSyndicationItem();
        }
        public PackageDocument ToPackageDocument()
        {
            return new PackageDocument
            {
                    CanPublish = false,
                    Packages = this.Items.Cast<NuGetSyndicationItem>().Select(x => x.ToPackage()).ToList()
            };
        }
    }
}