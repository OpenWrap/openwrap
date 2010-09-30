using System;
using System.Collections.Generic;

namespace OpenWrap.Build.BuildEngines
{
    public interface IPackageBuilder
    {
        IEnumerable<BuildResult> Build();
    }
}