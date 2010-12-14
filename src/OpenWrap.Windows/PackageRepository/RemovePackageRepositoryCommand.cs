using System;
using OpenWrap.Windows.Framework;
using ORRemoveRemoteCommand = OpenWrap.Commands.Remote.RemoveRemoteCommand;

namespace OpenWrap.Windows.PackageRepository
{
    class RemovePackageRepositoryCommand : CommandBase<NewPackageRepositoryViewModel>
    {
        protected override void Execute(NewPackageRepositoryViewModel parameter)
        {
            ORRemoveRemoteCommand removeRemoteCommand = new ORRemoveRemoteCommand
            {
                Name = parameter.Name
            };

            _commandOutput = removeRemoteCommand.Execute();
        }
    }
}
