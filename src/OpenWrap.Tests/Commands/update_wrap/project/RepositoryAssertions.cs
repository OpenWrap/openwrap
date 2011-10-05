using System.Linq;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.update_wrap.project
{
    public static class RepositoryAssertions
    {
        public static T ShouldHavePackage<T>(this T repository, string name, string version)
                where T : IPackageRepository
        {
            repository.PackagesByName[name].Count().ShouldBeGreaterThan(0);
            repository.HasPackage(name, version).ShouldBeTrue();
            return repository;
        }
        public static T ShouldNotHavePackage<T>(this T repository, string name, string version)
                where T : IPackageRepository
        {
            repository.HasPackage(name, version).ShouldBeFalse();
            return repository;
        }
    }
}