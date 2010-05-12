using System;
using System.IO;
using System.Linq;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Sources;

namespace OpenWrap.Repositories
{
    public class WrapListFileRepository : IWrapRepository
    {
        public WrapListFileRepository(IHttpNavigator navigator)
        {
            var document = navigator.LoadFileList();

            PackagesByName = (from wrapList in document.Descendants("wrap")
                              let name = wrapList.Attribute("name")
                              let version = wrapList.Attribute("version")
                              let link = (from link in wrapList.Elements("link")
                                          let relAttribute = link.Attribute("rel")
                                          where relAttribute != null && relAttribute.Value.Equals("package", StringComparison.OrdinalIgnoreCase)
                                          select relAttribute).FirstOrDefault()
                              where name != null && version != null && link != null
                              let depends = wrapList.Elements("depends").Select(x => x.Value)
                              select new HttpWrapPackageInfo(navigator, name.Value, version.Value, link.Value, depends))
                .Cast<IWrapPackageInfo>().ToLookup(x => x.Name);
        }
        public ILookup<string, IWrapPackageInfo> PackagesByName { get; private set; }

        public IWrapPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }
    }
}