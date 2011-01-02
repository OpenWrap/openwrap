using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Package;

namespace OpenWrap.Windows.PackageRepository
{
    public class PackageRepositoryViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageGroupViewModel> _packageGroups = new ObservableCollection<PackageGroupViewModel>();
        private readonly ICommand _removeCommand;
        private string _packagesCountText;
 
        public PackageRepositoryViewModel()
        {
            _removeCommand = new RemovePackageRepositoryCommand();
        }

        public string Name { get; set; }
        
        public string PackagesCountText 
        { 
            get
            {
                return _packagesCountText;
            } 
            set
            {
               if (_packagesCountText != value)
               {
                   _packagesCountText = value;
                   RaisePropertyChanged(() => this.PackagesCountText);
               }
            }
        }

        public Visibility ShowRemoveButton
        {
            get { return IsSystemRepo() ? Visibility.Hidden : Visibility.Visible; }
        }

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

        public void UpdatePackagesCountText()
        {
            PackagesCountText = GeneratePackagesCountText();
        }

        private bool IsSystemRepo()
        {
            return string.Equals(Name, "openwrap", StringComparison.OrdinalIgnoreCase);
        }
        
        private string GeneratePackagesCountText()
        {
            int packageCount = PackageGroups.Count;
            int allVersionsCount = PackageGroups.Sum(pg => pg.Versions.Count);
            string versions = allVersionsCount == 1 ? "version" : "versions";
            string packageGroups = packageCount == 1 ? "package" : "packages";

            return string.Format("Contains {0} {1} of {2} {3}", allVersionsCount, versions, packageCount, packageGroups);
        }
    }
}
