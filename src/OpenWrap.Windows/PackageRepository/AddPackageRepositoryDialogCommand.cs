using System;
using System.Windows;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryDialogCommand : CommandBase<Window>
    {
        protected override void Execute(Window parameter)
        {
            AddPackageRepositoryViewModel viewModel = new AddPackageRepositoryViewModel();
            AddPackageRepositoryWindow addPackageDialog = new AddPackageRepositoryWindow();
            addPackageDialog.CenterInParent(parameter);
            addPackageDialog.DataContext = viewModel;
            addPackageDialog.Show();
        }
    }
}
