using System;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class ViewWrapCommandOutput : GenericMessage
    {
        readonly IPackageInfo _result;

        public ViewWrapCommandOutput(IPackageInfo result) : base(string.Empty)
        {
            _result = result;
        }

        public override string ToString()
        {
            return string.Format("name: {0} \r\ndescription: {1} \r\nversion: {2}\r\ndependencies: {3}",
                                 _result.Name,
                                 _result.Description,
                                 _result.Version,
                                 _result.Dependencies.Select(x => x.ToString()).Join(", "));
        }
    }
}