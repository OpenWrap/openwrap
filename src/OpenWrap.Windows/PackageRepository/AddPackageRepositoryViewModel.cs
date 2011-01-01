using System.Windows;
using System.Windows.Input;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryViewModel : ViewModelBase
    {
        private readonly ICommand _closeCommand;
        private readonly ICommand _addRepositoryCommand = new AddPackageRepositoryCommand();
        private readonly ICommand _addRepositoryAndCloseCommand;

        private string _repositoryUrl;
        private string _repositoryName;

        public AddPackageRepositoryViewModel()
        {
            _closeCommand = new ActionCommand<Window>(w => w.Close());
            _addRepositoryAndCloseCommand = new ActionCommand<AddPackageRepositoryWindow>(AddRepositoryAndClose);
        }

        public ICommand AddRepositoryAndCloseCommand
        {
            get { return _addRepositoryAndCloseCommand; }
        }

        public ICommand AddRepositoryCommand
        {
            get { return _addRepositoryCommand; }
        }

        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public string RepositoryUrl
        {
            get
            {
                return _repositoryUrl;
            }
            set
            {
                if (_repositoryUrl != value)
                {
                    _repositoryUrl = value;
                    RaisePropertyChanged(() => this.RepositoryUrl);
                }
            }
        }

        public string RepositoryName
        {
            get
            {
                return _repositoryName;
            }
            set
            {
                if (_repositoryName != value)
                {
                    _repositoryName = value;
                    RaisePropertyChanged(() => this.RepositoryName);
                }
            }
        }

        static void AddRepositoryAndClose(AddPackageRepositoryWindow window)
        {
            AddPackageRepositoryViewModel vm = (AddPackageRepositoryViewModel)window.DataContext;
            vm.AddRepositoryCommand.Execute(vm);
            window.Close();
        }
    }
}
