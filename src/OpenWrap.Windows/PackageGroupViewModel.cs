using System;
using System.Collections.ObjectModel;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows
{
    public class PackageGroupViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageViewModel> _versions = new ObservableCollection<PackageViewModel>();

        public string Name { get; set; }

        public string Description
        {
            get
            {
                if (Versions.Count == 0)
                {
                    return string.Empty;
                }

                return Versions[0].Description;
            }
        }

        public ObservableCollection<PackageViewModel> Versions
        {
            get { return _versions; }
        }
    }
}
