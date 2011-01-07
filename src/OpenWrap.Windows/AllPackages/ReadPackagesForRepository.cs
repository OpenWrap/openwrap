using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;

namespace OpenWrap.Windows.AllPackages
{
    /// <summary>
    /// Read packages from a repository
    /// This can be slow as repos are often on the other side of the internet
    /// and can contain any number of packages and versions
    /// This task is isolated so that it can be done once and in parallel
    /// </summary>
    public class ReadPackagesForRepository : CommandBase<IPackageRepository>
    {
        protected override void Execute(IPackageRepository parameter)
        {
            List<PackageViewModel> packages = GetPackages(parameter);

            LoadedPackagesFromRepository resultData = new LoadedPackagesFromRepository
            {
                    Repository = parameter,
                    Packages = packages
            };

            Messenger.Default.Send(MessageNames.LoadedPackagesFromRepository, resultData);
        }

        private static List<PackageViewModel> GetPackages(IPackageRepository repository)
        {
            List<PackageViewModel> packages = new List<PackageViewModel>();

            foreach (IGrouping<string, IPackageInfo> packageGroup in repository.PackagesByName.NotNull())
            {
                IPackageInfo latestVersion = GetLatestVersion(packageGroup);
                PackageViewModel packageViewModel = new PackageViewModel
                {
                        Name = latestVersion.Name,
                        Description = latestVersion.Description,
                        Source = repository.Name,
                        Created = latestVersion.Created,
                        LatestVersion = latestVersion.Version,
                        VersionCount = packageGroup.Count()
                };  
              
                packages.Add(packageViewModel);
            }
            return packages;
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
    }
}
