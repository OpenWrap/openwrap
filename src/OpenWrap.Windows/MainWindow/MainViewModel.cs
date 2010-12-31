using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.NounVerb;
using OpenWrap.Windows.Package;
using OpenWrap.Windows.PackageRepository;

namespace OpenWrap.Windows.MainWindow
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageRepositoryViewModel> _packageRepositories = new ObservableCollection<PackageRepositoryViewModel>();
        private readonly ObservableCollection<PackageViewModel> _systemPackages = new ObservableCollection<PackageViewModel>();
        private readonly ObservableCollection<PackageViewModel> _projectPackages = new ObservableCollection<PackageViewModel>();
        private readonly ObservableCollection<NounSlice> _nouns = new ObservableCollection<NounSlice>();

        private readonly ICommand _addPackageRepositoryDialogCommand = new AddPackageRepositoryDialogCommand();
        private readonly ICommand _populateDataCommand = new PopulateMainData();

        private NounSlice _selectedNoun;

        public ObservableCollection<NounSlice> Nouns
        {
            get
            {
                return _nouns;
            }
       }

        public ObservableCollection<PackageViewModel> SystemPackages
        {
            get { return _systemPackages; }
        }

        public ObservableCollection<PackageViewModel> ProjectPackages
        {
            get { return _projectPackages; }
        }

        public ObservableCollection<PackageRepositoryViewModel> PackageRepositories
        {
            get { return _packageRepositories; }
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

        public ICommand AddPackageRepositoryDialogCommand
        {
            get { return _addPackageRepositoryDialogCommand; }
        }

        public void PopulateData()
        {
            _populateDataCommand.Execute(this);
        }
    }
}
