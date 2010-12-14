using System;
using OpenWrap.Windows.Framework;
using ORAddRemoteCommand = OpenWrap.Commands.Remote.AddRemoteCommand;

namespace OpenWrap.Windows.PackageRepository
{
    public class AddPackageRepositoryCommand : CommandBase<NewPackageRepositoryViewModel>
    {
        protected override void Execute(NewPackageRepositoryViewModel parameter)
        {
            ORAddRemoteCommand orCommand = new ORAddRemoteCommand
            {
                    Name = parameter.Name, 
                    Href = new Uri(parameter.Uri)
            };

            _commandOutput = orCommand.Execute();
        }
    }
}
