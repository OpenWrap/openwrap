using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using OpenWrap.Runtime;
using OpenWrap.Windows.AllPackages;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;

namespace OpenWrap.Windows.PackageRepository
{
    public class PackageRepositoriesViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageRepositoryViewModel> _packageRepositories = new ObservableCollection<PackageRepositoryViewModel>();
        
        private readonly ICommand _addPackageRepositoryDialogCommand = new AddPackageRepositoryDialogCommand();

        public PackageRepositoriesViewModel()
        {
            Messenger.Default.Subcribe(MessageNames.RepositoryListChanged, this, Populate);
            Messenger.Default.Subcribe<LoadedPackagesFromRepository>(MessageNames.LoadedPackagesFromRepository, this, LoadedPackages);
        }

        public ObservableCollection<PackageRepositoryViewModel> PackageRepositories
        {
            get { return _packageRepositories; }
        }

        public ICommand AddPackageRepositoryDialogCommand
        {
            get { return _addPackageRepositoryDialogCommand; }
        }

        private static IEnvironment GetEnvironment()
        {
            var environment = new CurrentDirectoryEnvironment();
            environment.Initialize();
            return environment;
        }

        private void Populate()
        {
            PopulateRepositoryData populateRepositoryData = new PopulateRepositoryData(GetEnvironment());
            populateRepositoryData.Execute(this);
        }

        private void LoadedPackages(LoadedPackagesFromRepository data)
        {
            PackageRepositoryViewModel repositoryViewModel = _packageRepositories.FirstOrDefault(repo => repo.Name == data.Repository.Name);

            if (repositoryViewModel != null)
            {
                WpfHelpers.DispatchToMainThread(() => repositoryViewModel.SetPackages(data.Packages));
            }
        }
    }
}
