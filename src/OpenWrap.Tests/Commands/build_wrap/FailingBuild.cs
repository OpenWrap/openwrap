using System.Collections.Generic;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;

namespace Tests.Commands.build_wrap
{
    public class FailingBuild : IPackageBuilder
    {
        public IEnumerable<BuildResult> Build()
        {
            yield return new ErrorBuildResult();
        }
    }
}