using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement
{
    public class PackageFoundCommandOutput : GenericMessage
    {
        readonly PackageFoundResult _result;

        public PackageFoundCommandOutput(PackageFoundResult result)
                : base(" - {0} (available: {1})", result.Name, result.Packages.Select(x => x.Version + (x.Nuked ? " [nuked]" : string.Empty)).Join(", "))
        {
            _result = result;
        }

        public string Name
        {
            get { return _result.Name; }
        }
    }
}