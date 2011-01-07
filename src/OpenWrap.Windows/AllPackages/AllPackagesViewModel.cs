using System;
using System.Collections.ObjectModel;
using OpenWrap.Collections;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;

namespace OpenWrap.Windows.AllPackages
{
    public class AllPackagesViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageViewModel> _allPackages = new ObservableCollection<PackageViewModel>();

        public AllPackagesViewModel()
        {
            Messenger.Default.Subcribe(MessageNames.PackageListChanged, this, Populate);
            Messenger.Default.Subcribe(MessageNames.RepositoryListChanged, this, Populate);
            Messenger.Default.Subcribe<LoadedPackagesFromRepository>(MessageNames.LoadedPackagesFromRepository, this, LoadedPackages);
        }

        public ObservableCollection<PackageViewModel> AllPackages { get { return _allPackages; } }

        private void Populate()
        {
            // this command is async, data comes back via LoadedPackagesFromRepository messages
            ReadPackagesForRepositories command = new ReadPackagesForRepositories();
            command.Execute(this);
        }

        private void LoadedPackages(LoadedPackagesFromRepository data)
        {
            ReadPackageLocalStateCommand localStateCommand = new ReadPackageLocalStateCommand();
            localStateCommand.Execute(data);

            WpfHelpers.DispatchToMainThread(() => _allPackages.AddRange(data.Packages));
        }
    }
}
