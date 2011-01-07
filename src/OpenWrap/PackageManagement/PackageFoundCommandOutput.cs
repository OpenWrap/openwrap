using System;
using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement
{
    public class PackageFoundCommandOutput : GenericMessage
    {
        readonly PackageFoundResult _result;

        private static string createOutput(PackageFoundResult result)
        {
            var str = string.Format(" - {0} (available: {1})",
                                 result.Name,
                                 result.Packages.Select(x => x.Version + (x.Nuked ? " [nuked]" : string.Empty)).Join(", "));
            if (result.HasDetail)
                str += string.Format("{0}   {1}", Environment.NewLine, result.Description);

            return str;
        }

        public PackageFoundCommandOutput(PackageFoundResult result) : base(createOutput(result))
        {
            _result = result;
        }

        public string Name
        {
            get { return _result.Name; }
        }
    }
}