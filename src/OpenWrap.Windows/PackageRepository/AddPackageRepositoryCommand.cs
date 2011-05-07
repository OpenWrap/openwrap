using System;
using OpenWrap.Commands.Remote;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryCommand : CommandBase<AddPackageRepositoryViewModel>
    {
        protected override bool CanExecute(AddPackageRepositoryViewModel parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(parameter.RepositoryName.TrimNullSafe()))
            {
                return false;
            }

            if (string.IsNullOrEmpty(parameter.RepositoryUrl.TrimNullSafe()))
            {
                return false;
            }

            return true;
        }

        protected override void Execute(AddPackageRepositoryViewModel parameter)
        {
            throw new NotImplementedException();
            AddRemoteCommand addRemoteCommand = new AddRemoteCommand
            {
                    Name = parameter.RepositoryName, 
                    //Href = new Uri(parameter.RepositoryUrl)
            };

            CommandHelper.ExecuteAndSend(addRemoteCommand);
            Messenger.Default.Send(MessageNames.RepositoryListChanged);
        }
    }
}
