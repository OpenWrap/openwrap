using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;

namespace Tests.Commands.build_wrap
{
    public class NullPackageBuilder : IPackageBuilder
    {
        public static Func<IEnumerable<BuildResult>> Build = () =>
        {
            Called = true;
            return Enumerable.Empty<BuildResult>();
        };

        public static bool Called;

        IEnumerable<BuildResult> IPackageBuilder.Build()
        {
            return Build();
        }
    }
}