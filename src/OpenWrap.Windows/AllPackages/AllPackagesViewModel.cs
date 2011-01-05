using System;
using System.Collections.ObjectModel;
using System.Threading;
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
        }

        public ObservableCollection<PackageViewModel> AllPackages { get { return _allPackages; } }

        private void Populate()
        {
            ThreadPool.QueueUserWorkItem(PopulateAsync);
        }

        private void PopulateAsync(object required)
        {
            PopulateAllPackagesList command = new PopulateAllPackagesList();
            command.Execute(this);
        }
    }
}
