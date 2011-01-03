using System;
using OpenWrap.Windows.Framework;
using OpenWrapRemoveRemoteCommand = OpenWrap.Commands.Remote.RemoveRemoteCommand;

namespace OpenWrap.Windows.PackageRepository
{
    class RemovePackageRepositoryCommand : CommandBase<PackageRepositoryViewModel>
    {
        protected override void Execute(PackageRepositoryViewModel parameter)
        {
            OpenWrapRemoveRemoteCommand removeRemoteCommand = new OpenWrapRemoveRemoteCommand
            {
                Name = parameter.Name
            };

            CommandOutput = removeRemoteCommand.Execute();
        }
    }
}
