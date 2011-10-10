using System.Collections.Generic;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;

namespace OpenWrap.Commands.Wrap
{
    class ErrorPackageBuilder : IPackageBuilder
    {
        readonly string _message;

        public ErrorPackageBuilder(string message)
        {
            _message = message;
        }

        public IEnumerable<BuildResult> Build()
        {
            yield return new ErrorBuildResult(_message);
        }
    }
}