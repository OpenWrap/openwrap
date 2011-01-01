using System;
using System.Linq;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;
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

            CommandOutput = removeRemoteCommand.Execute().ToList();

            Messenger.Default.Send("PackageListChanged");
        }
    }
}
