using System;
using System.Linq;
using OpenWrap.Windows.Framework;
using OpenWrapAddRemoteCommand = OpenWrap.Commands.Remote.AddRemoteCommand;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryCommand : CommandBase<NewPackageRepositoryViewModel>
    {
        protected override void Execute(NewPackageRepositoryViewModel parameter)
        {
            OpenWrapAddRemoteCommand openWrapCommand = new OpenWrapAddRemoteCommand
            {
                    Name = parameter.RepositoryName, 
                    Href = new Uri(parameter.RepositoryUrl)
            };

            CommandOutput = openWrapCommand.Execute().ToList();
        }
    }
}
