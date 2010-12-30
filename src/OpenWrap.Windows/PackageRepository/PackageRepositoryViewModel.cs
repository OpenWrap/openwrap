using System.Collections.ObjectModel;
using System.Windows.Input;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Package;

namespace OpenWrap.Windows.PackageRepository
{
    public class PackageRepositoryViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageGroupViewModel> _packageGroups = new ObservableCollection<PackageGroupViewModel>();
        private readonly ICommand _removeCommand;

        public PackageRepositoryViewModel()
        {
            _removeCommand = new RemovePackageRepositoryCommand();
        }

        public string Name { get; set; }

        public ObservableCollection<PackageGroupViewModel> PackageGroups
        {
            get
            {
                return _packageGroups;
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand;
            }
        }
    }
}
