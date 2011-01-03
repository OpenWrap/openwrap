using System;
using System.Collections.ObjectModel;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.AllPackages
{
    public class AllPackagesViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageViewModel> _allPackages = new ObservableCollection<PackageViewModel>();

        public ObservableCollection<PackageViewModel> AllPackages { get { return _allPackages; } }
    }
}
