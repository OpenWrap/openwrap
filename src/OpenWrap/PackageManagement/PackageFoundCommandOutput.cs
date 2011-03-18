using System;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class PackageFoundCommandOutput : GenericMessage
    {
        readonly PackageFoundResult _result;

        public PackageFoundCommandOutput(PackageFoundResult result)
                : base(string.Empty)
        {
            _result = result;
        }

        public string Name
        {
            get { return _result.Name; }
        }

        public override string ToString()
        {
            var versionInfo = _result.Packages.Select(x => x.Version + (x.Nuked ? " [nuked]" : string.Empty)).Join(", ");
            var detailedInfo = shouldShowDescription() ? description() : string.Empty;

            var currentInfo  = CurrentInfo();

            return string.Format(" - {0} {2}({3}available: {1})", Name, versionInfo, detailedInfo, currentInfo);
        }

        string CurrentInfo()
        {
            var item = _result.CurrentPackages.Where(x => x.Name.Equals(Name)).LastOrDefault();

            if (item == null)
                return string.Empty;

            return string.Format("current: {0} ", item.Version);
        }

        string description()
        {
            var description = _result.Packages.Last().Description;
            return !string.IsNullOrEmpty(description) ? string.Format("\r\n{0}\r\n", description) : string.Empty;
        }

        bool shouldShowDescription()
        {
            return _result.Options.Equals(PackageListOptions.Detailed);
        }
    }
}