using System;
using System.IO;
using System.Linq;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.IO;

namespace OpenWrap.Repositories
{
    public interface IPackageRepository
    {
        ILookup<string, IPackageInfo> PackagesByName { get; }
        IPackageInfo Find(WrapDependency dependency);
        IPackageInfo Publish(string packageFileName, Stream packageStream);
    }
}