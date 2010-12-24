using System.Collections.Generic;

namespace OpenWrap.Build.PackageBuilders
{
    public interface IPackageBuilder
    {
        IEnumerable<BuildResult> Build();
    }
}