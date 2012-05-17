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
        SemanticVersion SemanticVersion { get; }

        [Obsolete("Please use the new SemanticVersion instead")]
        Version Version { get; }

        IPackageRepository Source { get; }
        string FullName { get; }
        string Description { get; }
        string Title { get; }
        DateTimeOffset Created { get; }

        bool Anchored { get; }

        bool Nuked { get; }
        bool IsValid { get; }
        IPackage Load();
    }
    
}