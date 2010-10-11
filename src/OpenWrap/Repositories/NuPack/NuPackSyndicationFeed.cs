using System.Linq;
using System.ServiceModel.Syndication;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuPack
{
    public class NuPackSyndicationFeed : SyndicationFeed
    {
        protected override SyndicationItem CreateItem()
        {
            return new NuPackSyndicationItem();
        }
        public PackageDocument ToPackageDocument()
        {
            return new PackageDocument
            {
                    CanPublish = false,
                    Packages = this.Items.Cast<NuPackSyndicationItem>().Select(x => x.ToPackage()).ToList()
            };
        }
    }
}