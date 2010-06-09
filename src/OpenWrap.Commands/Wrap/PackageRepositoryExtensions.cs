using System;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    public static class PackageRepositoryExtensions
    {
        public static bool HasDependency(this IPackageRepository packageRepository, string name, Version version)
        {
            return packageRepository.Find(new WrapDependency { Name = name, VersionVertices = { new ExactVersionVertice(version) } }) != null;
        }
    }
}