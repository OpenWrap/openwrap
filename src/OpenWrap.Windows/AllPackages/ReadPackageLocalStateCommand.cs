using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.AllPackages
{
    public class ReadPackageLocalStateCommand : CommandBase<LoadedPackagesFromRepository>
    {
        protected override void Execute(LoadedPackagesFromRepository parameter)
        {
            IEnvironment environment = GetEnvironment();

            var systemPackages = GetRepositoryPackages(environment.SystemRepository);
            TestPackageIsInstalled(systemPackages, parameter.Packages,
                (pack, value) => pack.IsInstalledSystem = value);

            var localPackages = GetRepositoryPackages(environment.CurrentDirectoryRepository);
            TestPackageIsInstalled(localPackages, parameter.Packages,
                (pack, value) => pack.IsInstalledInDirectory = value);
        }

        private static void TestPackageIsInstalled(
            IEnumerable<IPackageInfo> repositoryPackages,
            IEnumerable<PackageViewModel> packages,
            Action<PackageViewModel, bool> setAction)
        {
            foreach (var package in packages)
            {                
                PackageViewModel packageItem = package;
                bool isInstalled = repositoryPackages.Any(rp => rp.Name == packageItem.Name);
                setAction(packageItem, isInstalled);
            }
        }

        static IEnumerable<IPackageInfo> GetRepositoryPackages(IPackageRepository packageRepository)
        {
            var repositoryPackageGroups = packageRepository.PackagesByName.NotNull();
            return repositoryPackageGroups.SelectMany(rpg => rpg.ToList());
        }

        private static IEnvironment GetEnvironment()
        {
            var environment = new CurrentDirectoryEnvironment();
            environment.Initialize();
            return environment;
        }
    }
}
