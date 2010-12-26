using System.Collections.Generic;
using OpenWrap.Runtime;

namespace OpenWrap.Build.PackageBuilders
{
    public class NullPackageBuilder : IPackageBuilder
    {
        readonly IEnvironment _environment;

        public NullPackageBuilder(IEnvironment environment)
        {
            _environment = environment;
        }

        public IEnumerable<BuildResult> Build()
        {
            yield break;
        }
    }
}