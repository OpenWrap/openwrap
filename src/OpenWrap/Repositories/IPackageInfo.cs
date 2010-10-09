using System;
using System.Collections.Generic;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public interface IPackageInfo
    {
        ICollection<PackageDependency> Dependencies { get; }
        string Name { get; }
        Version Version { get; }
        IPackage Load();
        IPackageRepository Source { get; }
        string FullName { get; }
        DateTime? LastModifiedTimeUtc { get; }
        bool Anchored { get; }
    }
}