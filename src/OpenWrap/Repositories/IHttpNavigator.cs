using System.IO;
using System.Xml.Linq;

namespace OpenWrap.Repositories
{
    public interface IHttpNavigator
    {
        XDocument LoadFileList();
        Stream LoadFile(string href);
    }
}