using System.Windows.Input;
using OpenWrap.Windows.Controls;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.PackageRepository
{
    public class NewPackageRepositoryViewModel : ViewModelBase
    {
        private readonly ICommand _cancelCommand;
        private readonly ICommand _addRepositoryCommand = new AddPackageRepositoryCommand();

        private string _repositoryUrl;
        private string _repositoryName;

        public NewPackageRepositoryViewModel()
        {
            _cancelCommand = new ActionCommand<AddRepositoryWindow>(w => w.Close());
        }

        public ICommand AddRepositoryCommand
        {
            get { return _addRepositoryCommand; }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
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
                    RaisePropertyChanged<NewPackageRepositoryViewModel>(o => o.RepositoryUrl);
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
                    RaisePropertyChanged<NewPackageRepositoryViewModel>(o => o.RepositoryName);
                }
            }
        }
    }
}
