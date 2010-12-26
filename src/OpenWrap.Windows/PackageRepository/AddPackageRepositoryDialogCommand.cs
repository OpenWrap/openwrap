using System;
using OpenWrap.Windows.Controls;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryDialogCommand : CommandBase<NewPackageRepositoryViewModel>
    {
        protected override void Execute(NewPackageRepositoryViewModel parameter)
        {
            NewPackageRepositoryViewModel viewModel = new NewPackageRepositoryViewModel();
            AddRepositoryWindow window = new AddRepositoryWindow();
            window.DataContext = viewModel;
            window.Show();
        }
    }
}
