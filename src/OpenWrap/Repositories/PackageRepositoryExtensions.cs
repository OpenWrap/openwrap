using System;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public static class PackageRepositoryExtensions
    {
        public static bool HasDependency(this IPackageRepository packageRepository, string name, Version version)
        {
            return packageRepository.Find(new WrapDependency { Name = name, VersionVertices = { new ExactVersionVertice(version) } }) != null;
        }
    }
}