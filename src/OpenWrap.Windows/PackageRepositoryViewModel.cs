using System;
using System.Collections.ObjectModel;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows
{
    public class PackageRepositoryViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageViewModel> _packages = new ObservableCollection<PackageViewModel>();

        public string Name { get; set; }

        public ObservableCollection<PackageViewModel> Packages
        {
            get
            {
                return _packages;
            }
        }
    }
}
