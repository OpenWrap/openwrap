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
    public class PopulateAllPackagesList : CommandBase<AllPackagesViewModel>
    {
        protected override void Execute(AllPackagesViewModel parameter)
        {
            // todo: constructor-inject services
            IEnvironment environment = GetEnvironment();

            ReadPackagesFromRepositories(environment.RemoteRepositories, parameter.AllPackages);

            ReadPackageState(environment.SystemRepository,  
                parameter.AllPackages, 
                (pack, value) => pack.IsInstalledSystem = value);

            ReadPackageState(environment.CurrentDirectoryRepository,  
                parameter.AllPackages, 
                (pack, value) => pack.IsInstalledInDirectory = value);
        }

        private static void ReadPackageState(
            IPackageRepository packageRepository, 
            IEnumerable<PackageViewModel> allPackages, 
            Action<PackageViewModel, bool> setAction)
        {
            var repositoryPackageGroups = packageRepository.PackagesByName.NotNull();
            var repositoryPackages = repositoryPackageGroups.SelectMany(rpg => rpg.ToList());
            foreach (PackageViewModel package in allPackages)
            {
                PackageViewModel packageItem = package;
                bool isInstalled = repositoryPackages.Any(rp => rp.Name == packageItem.Name);
                setAction(packageItem, isInstalled);
            }
        }

        private static void ReadPackagesFromRepositories(IEnumerable<IPackageRepository> repositories, IList<PackageViewModel> results)
        {
            foreach (IPackageRepository repository in repositories)
            {
                foreach (IGrouping<string, IPackageInfo> packageGroup in repository.PackagesByName.NotNull())
                {
                    IPackageInfo latestVersion = GetLatestVersion(packageGroup);
                    PackageViewModel packageViewModel = new PackageViewModel
                        {
                            Name = latestVersion.Name,
                            Description = latestVersion.Description,
                            Source = repository.Name,
                            Created = latestVersion.Created,
                            LatestVersion = latestVersion.Version
                        };

                    results.Add(packageViewModel);
                }
            }
        }

        private static IPackageInfo GetLatestVersion(IEnumerable<IPackageInfo> packageGroup)
        {
            IPackageInfo result = null;

            foreach (IPackageInfo item in packageGroup)
            {
                if (result == null || item.Version > result.Version)
                {
                    result = item;
                }
            }

            return result;
        }

        private static IEnvironment GetEnvironment()
        {
            var environment = new CurrentDirectoryEnvironment();
            environment.Initialize();
            return environment;
        }
    }
}
