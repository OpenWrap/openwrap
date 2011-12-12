using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.PackageModel
{
    public interface IPackageInfo
    {
        PackageIdentifier Identifier { get; }
        ICollection<PackageDependency> Dependencies { get; }
        string Name { get; }
        SemanticVersion Version { get; }
        IPackage Load();
        IPackageRepository Source { get; }
        string FullName { get; }
        string Description { get; }
        string Title { get; }
        DateTimeOffset Created { get; }
        
        bool Anchored { get; }

        bool Nuked { get; }
        bool IsValid { get; }
    }
}