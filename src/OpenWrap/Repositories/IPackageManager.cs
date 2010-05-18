using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    public interface IPackageManager : IService
    {
        IEnumerable<PackageResolution> TryResolveDependencies(WrapDescriptor wrapDescriptor, IPackageRepository projectRepository, IPackageRepository userRepository, IEnumerable<IPackageRepository> remoteRepositories);
    }

    public class PackageResolution
    {
        public string Name { get; set; }
        public IEnumerable<IPackageInfo> Versions { get; set; }
        public bool HasConflicts { get { return Versions.Count() > 1; } }
    }
}
