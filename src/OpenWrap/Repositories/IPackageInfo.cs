using System;
using System.Collections.Generic;
using OpenRasta.Wrap.Dependencies;

namespace OpenRasta.Wrap.Sources
{
    public interface IPackageInfo
    {
        ICollection<WrapDependency> Dependencies { get; }
        string Name { get; }
        Version Version { get; }
        IPackage Load();
    }
}