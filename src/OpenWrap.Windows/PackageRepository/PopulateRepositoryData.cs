using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Package;

namespace OpenWrap.Windows.PackageRepository
{
    public class PopulateRepositoryData : CommandBase<PackageRepositoriesViewModel>
    {
        private readonly IEnvironment _environment;

        public PopulateRepositoryData(IEnvironment environment)
        {
            _environment = environment;
        }

        protected override void Execute(PackageRepositoriesViewModel parameter)
        {
            if (_environment != null)
            {
                WpfHelpers.DispatchToMainThread(() => parameter.PackageRepositories.Clear());
                ReadPackageRepositories(_environment.RemoteRepositories, parameter.PackageRepositories);
            }
        }

        private static void ReadPackageRepositories(
            IEnumerable<IPackageRepository> remoteRepositories,
            IList<PackageRepositoryViewModel> results)
        {

            foreach (var packageRepository in remoteRepositories)
            {
                PackageRepositoryViewModel viewModel = new PackageRepositoryViewModel
                {
                    Name = packageRepository.Name
                };

                IEnumerable<PackageGroupViewModel> packages = TranslateAndGroupPackages(packageRepository.PackagesByName.NotNull());
                viewModel.PackageGroups.AddRange(packages);
                viewModel.UpdatePackagesCountText();

                 WpfHelpers.DispatchToMainThread(() => results.Add(viewModel));
            }
        }

        private static IEnumerable<PackageGroupViewModel> TranslateAndGroupPackages(IEnumerable<IGrouping<string, IPackageInfo>> packageGroups)
        {
            List<PackageGroupViewModel> result = new List<PackageGroupViewModel>();

            foreach (IGrouping<string, IPackageInfo> packageGroup in packageGroups)
            {
                PackageGroupViewModel packageGroupViewModel = new PackageGroupViewModel();
                packageGroupViewModel.Name = packageGroup.Key;
                foreach (var packageInfo in packageGroup)
                {
                    PackageViewModel viewModel = TranslatePackage(packageInfo);
                    packageGroupViewModel.Versions.Add(viewModel);
                }

                result.Add(packageGroupViewModel);
            }

            return result;
        }

        private static PackageViewModel TranslatePackage(IPackageInfo packageInfo)
        {
            return new PackageViewModel
            {
                Name = packageInfo.Name,
                FullName = packageInfo.FullName,
                Description = packageInfo.Description,
                ShortVersion = packageInfo.Version.ToString(),
                Version = "Version " + packageInfo.Version,
                Created = packageInfo.Created,
                Anchored = packageInfo.Anchored,
                Nuked = packageInfo.Nuked
            };
        }
    }
}
