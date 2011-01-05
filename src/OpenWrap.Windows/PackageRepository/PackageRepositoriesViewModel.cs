using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using OpenWrap.Runtime;
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
            ThreadPool.QueueUserWorkItem(PopulateAsync);
        }

        private void PopulateAsync(object required)
        {
            PopulateRepositoryData populateRepositoryData = new PopulateRepositoryData(GetEnvironment());
            populateRepositoryData.Execute(this);
        }
    }
}
