using System;
using System.Collections.ObjectModel;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows
{
    public class PackageRepositoryViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageGroupViewModel> _packageGroups = new ObservableCollection<PackageGroupViewModel>();

        public string Name { get; set; }

        public ObservableCollection<PackageGroupViewModel> PackageGroups
        {
            get
            {
                return _packageGroups;
            }
        }
    }
}
