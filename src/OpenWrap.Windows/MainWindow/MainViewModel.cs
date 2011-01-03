using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OpenWrap.Windows.AllPackages;
using OpenWrap.Windows.CommandOutput;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.NounVerb;
using OpenWrap.Windows.PackageRepository;

namespace OpenWrap.Windows.MainWindow
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ObservableCollection<NounSlice> _nouns = new ObservableCollection<NounSlice>();

        private readonly AllPackagesViewModel _allPackages = new AllPackagesViewModel();
        private readonly PackageRepositoriesViewModel _packageRepositories = new PackageRepositoriesViewModel();

        readonly CommandOutputControlViewModel _commandOutput = new CommandOutputControlViewModel();

        private readonly ICommand _populateDataCommand = new PopulateMainData();

        private NounSlice _selectedNoun;

        public ObservableCollection<NounSlice> Nouns
        {
            get
            {
                return _nouns;
            }
       }

        public NounSlice SelectedNoun
        {
            get
            {
                return _selectedNoun;
            }
            set
            {
                _selectedNoun = value;
                RaisePropertyChanged(() => this.SelectedNoun);
            }
        }

        public CommandOutputControlViewModel CommandOutput
        {
            get { return _commandOutput; }
        }

        public PackageRepositoriesViewModel PackageRepositories
        {
            get { return _packageRepositories; }
        }

        public AllPackagesViewModel AllPackages
        {
            get { return _allPackages; }
        }

        public void PopulateData()
        {
            _populateDataCommand.Execute(this);
        }
    }
}
