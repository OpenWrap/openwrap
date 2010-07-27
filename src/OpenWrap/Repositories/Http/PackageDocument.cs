using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Repositories.Http
{
    public class PackageDocument
    {
        public IEnumerable<PackageItem> Packages { get; set; }
    }
}
