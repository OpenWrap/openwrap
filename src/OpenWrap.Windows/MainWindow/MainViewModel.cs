using System;
using OpenWrap.Windows.AllPackages;
using OpenWrap.Windows.CommandOutput;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.NounVerb;
using OpenWrap.Windows.PackageRepository;

namespace OpenWrap.Windows.MainWindow
{
    public class MainViewModel : ViewModelBase
    {
        readonly NounVerbViewModel _nounVerb = new NounVerbViewModel();
        private readonly AllPackagesViewModel _allPackages = new AllPackagesViewModel();
        private readonly PackageRepositoriesViewModel _packageRepositories = new PackageRepositoriesViewModel();

        private readonly CommandOutputControlViewModel _commandOutput = new CommandOutputControlViewModel();

        public NounVerbViewModel NounVerb
        {
            get { return _nounVerb; }
        }

        public AllPackagesViewModel AllPackages
        {
            get { return _allPackages; }
        }

        public PackageRepositoriesViewModel PackageRepositories
        {
            get { return _packageRepositories; }
        }

        public CommandOutputControlViewModel CommandOutput
        {
            get { return _commandOutput; }
        }
    }
}
