using System;
using System.Collections.Generic;

namespace OpenWrap.Dependencies
{
    public interface IPackageDependency
    {
        string Name { get; }
        bool Anchored { get; }
        bool ContentOnly { get; }
        IEnumerable<string> Tags { get; set; }
        IEnumerable<VersionVertex> VersionVertices { get; }
        bool IsFulfilledBy(Version version);
    }
}