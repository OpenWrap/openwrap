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
        }

        private static void ReadPackagesFromRepositories(IEnumerable<IPackageRepository> repositories, IList<PackageViewModel> results)
        {
            foreach (IPackageRepository repository in repositories)
            {
                foreach (IGrouping<string, IPackageInfo> packageGroup in repository.PackagesByName.NotNull())
                {
                    IPackage latestVersion = GetLatestVersion(packageGroup);
                    PackageViewModel packageViewModel = new PackageViewModel
                        {
                            Name = latestVersion.Name,
                            Description = latestVersion.Description,
                            Created = latestVersion.Created,
                            LatestVersion = latestVersion.Version
                        };

                    results.Add(packageViewModel);
                }
            }
        }

        private static IPackage GetLatestVersion(IEnumerable<IPackageInfo> packageGroup)
        {
            IPackage result = null;

            foreach (IPackage item in packageGroup)
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
