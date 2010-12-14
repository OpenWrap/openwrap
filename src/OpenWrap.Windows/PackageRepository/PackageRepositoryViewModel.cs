using System.Collections.ObjectModel;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Package;

namespace OpenWrap.Windows.PackageRepository
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
