using System;
using OpenWrap.Windows.Controls;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryDialogCommand : CommandBase<AddPackageRepositoryViewModel>
    {
        protected override void Execute(AddPackageRepositoryViewModel parameter)
        {
            AddPackageRepositoryViewModel viewModel = new AddPackageRepositoryViewModel();
            AddPackageRepositoryWindow window = new AddPackageRepositoryWindow();
            window.DataContext = viewModel;
            window.Show();
        }
    }
}
