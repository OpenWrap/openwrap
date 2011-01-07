using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using OpenWrap.Collections;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.AllPackages;

namespace OpenWrap.Windows.PackageRepository
{
    public class PackageRepositoryViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageViewModel> _packages = new ObservableCollection<PackageViewModel>();
        private readonly ICommand _removeCommand;
        private string _packagesCountText;
 
        public PackageRepositoryViewModel()
        {
            _removeCommand = new RemovePackageRepositoryCommand();
            UpdatePackagesCountText();
        }

        public string Name { get; set; }
        public bool PackagesLoaded { get; set; }
        
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

        public ObservableCollection<PackageViewModel> Packages
        {
            get
            {
                return _packages;
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand;
            }
        }

        public void SetPackages(IEnumerable<PackageViewModel> packages)
        {
            Packages.Clear();
            Packages.AddRange(packages);
            PackagesLoaded = true;
            UpdatePackagesCountText();
        }

        private void UpdatePackagesCountText()
        {
            PackagesCountText = GeneratePackagesCountText();
        }

        private bool IsSystemRepo()
        {
            return string.Equals(Name, "openwrap", StringComparison.OrdinalIgnoreCase);
        }
        
        private string GeneratePackagesCountText()
        {
            if (! PackagesLoaded)
            {
                return "Packages not yet loaded";
            }

            int packageCount = Packages.Count;
            int allVersionsCount = Packages.Sum(pack => pack.VersionCount);
            string versions = allVersionsCount == 1 ? "version" : "versions";
            string packageGroups = packageCount == 1 ? "package" : "packages";

            return string.Format("Contains {0} {1} of {2} {3}", allVersionsCount, versions, packageCount, packageGroups);
        }
    }
}
