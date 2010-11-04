using System;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public static class PackageRepositoryExtensions
    {
        public static bool HasPackage(this IPackageRepository packageRepository, string name, string version)
        {
            var typedVersion = new Version(version);
            return packageRepository.PackagesByName[name].Any(x => x.Version == typedVersion);
        }
        public static bool HasDependency(this IPackageRepository packageRepository, string name, Version version)
        {
            return packageRepository.Find(new PackageDependency { Name = name, VersionVertices = { new ExactVersionVertex(version) } }) != null;
        }

        public static void RefreshAnchors(this IPackageRepository repo, DependencyResolutionResult resolvedPackages)
        {
            var projectRepo = repo as ISupportAnchoring;
            if (projectRepo != null)
            {
                var packagesToAnchor = resolvedPackages.ResolvedPackages
                    .Where(x => x.Dependencies.Any(d=>d.Dependency.Anchored) || (x.Package != null && x.Package.Anchored))
                    .Select(x=>x.Package)
                    .NotNull()
                    .Where(x=>x.Source == projectRepo).ToList();

                projectRepo.VerifyAnchors(packagesToAnchor);
            }
        }
    }
}