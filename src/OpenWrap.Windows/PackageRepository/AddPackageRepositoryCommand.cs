using System;
using System.Linq;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;
using OpenWrapAddRemoteCommand = OpenWrap.Commands.Remote.AddRemoteCommand;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryCommand : CommandBase<AddPackageRepositoryViewModel>
    {
        protected override void Execute(AddPackageRepositoryViewModel parameter)
        {
            OpenWrapAddRemoteCommand openWrapCommand = new OpenWrapAddRemoteCommand
            {
                    Name = parameter.RepositoryName, 
                    Href = new Uri(parameter.RepositoryUrl)
            };

            CommandOutput = openWrapCommand.Execute().ToList();

            Messenger.Default.Send("PackageListChanged");
        }
    }
}
