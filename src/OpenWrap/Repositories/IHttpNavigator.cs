using System;
using System.IO;
using System.Xml.Linq;

namespace OpenWrap.Repositories
{
    // TODO: Yes, I do know this is evil and wrongly named.
    // So are the XmlPackage classes. Need rework but need bootstrapping.
    public interface IHttpNavigator
    {
        XDocument LoadFileList();
        Stream LoadFile(Uri href);
    }
}