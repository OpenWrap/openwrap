using System;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryDialogCommand : CommandBase<PackageRepositoryViewModel>
    {
        protected override void Execute(PackageRepositoryViewModel parameter)
        {
            AddPackageRepositoryViewModel viewModel = new AddPackageRepositoryViewModel();
            AddPackageRepositoryWindow window = new AddPackageRepositoryWindow();
            window.DataContext = viewModel;
            window.Show();
        }
    }
}
