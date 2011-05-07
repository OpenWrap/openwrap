using System;
using System.Collections.Generic;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Windows.Framework;

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
                parameter.PackageRepositories.Clear();
                //ReadPackageRepositories(_environment.RemoteRepositories, parameter.PackageRepositories);
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
                    Name = packageRepository.Name,
                    PackagesLoaded = false
                };

                results.Add(viewModel);
            }
        }
    }
}
