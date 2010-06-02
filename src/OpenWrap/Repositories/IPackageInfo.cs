using System;
using System.Collections.Generic;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public interface IPackageInfo
    {
        ICollection<WrapDependency> Dependencies { get; }
        string Name { get; }
        Version Version { get; }
        IPackage Load();
        IPackageRepository Source { get; }
        string FullName { get; }
    }
}