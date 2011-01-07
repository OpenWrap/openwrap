using System;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;
using OpenWrapRemoveRemoteCommand = OpenWrap.Commands.Remote.RemoveRemoteCommand;

namespace OpenWrap.Windows.PackageRepository
{
    class RemovePackageRepositoryCommand : CommandBase<PackageRepositoryViewModel>
    {
        protected override bool CanExecute(PackageRepositoryViewModel parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            string name = parameter.Name.TrimNullSafe();
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return true;
        }
        
        protected override void Execute(PackageRepositoryViewModel parameter)
        {
            OpenWrapRemoveRemoteCommand removeRemoteCommand = new OpenWrapRemoveRemoteCommand
            {
                Name = parameter.Name.TrimNullSafe()
            };

            CommandHelper.ExecuteAndSend(removeRemoteCommand);
            Messenger.Default.Send(MessageNames.RepositoryListChanged);
        }
    }
}
