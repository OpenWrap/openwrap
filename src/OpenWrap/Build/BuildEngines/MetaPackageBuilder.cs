using System.Collections.Generic;
using OpenFileSystem.IO.FileSystems.Local;

namespace OpenWrap.Build.BuildEngines
{
    public class MetaPackageBuilder : IPackageBuilder
    {
        readonly IEnvironment _environment;

        public MetaPackageBuilder(IEnvironment environment)
        {
            _environment = environment;
        }

        public IEnumerable<BuildResult> Build()
        {
            yield break;
        }
    }
}