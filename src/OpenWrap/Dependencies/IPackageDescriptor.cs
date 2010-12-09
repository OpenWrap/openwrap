using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
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