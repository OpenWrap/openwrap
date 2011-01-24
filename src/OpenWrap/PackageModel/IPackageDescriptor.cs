using System;
using System.Collections.Generic;

namespace OpenWrap.PackageModel
{
    public interface IPackageDescriptor : IEnumerable<IPackageDescriptorEntry>
    {
        bool Anchored { get; set; }
        DateTimeOffset Created { get; }
        ICollection<string> Build { get; }
        ICollection<PackageDependency> Dependencies { get; }
        string Title { get; set; }
        string Description { get; set; }
        string FullName { get; }
        PackageIdentifier Identifier { get; }
        string Name { get; set; }
        ICollection<PackageNameOverride> Overrides { get; }
        bool UseProjectRepository { get; set; }
        bool UseSymLinks { get; set; }
        Version Version { get; set; }
        string ReferencedAssemblies { get; set; }
        ICollection<string> DirectoryStructure { get; }
        IPackageDescriptor CreateScoped(IEnumerable<IPackageDescriptorEntry> read);
    }
}