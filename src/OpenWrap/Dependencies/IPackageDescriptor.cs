using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public interface IPackageDescriptor : IPackageInfo
    {
        string BuildCommand { get; set; }
        bool UseProjectRepository { get; set; }
        ICollection<PackageNameOverride> Overrides { get; }
    }
}