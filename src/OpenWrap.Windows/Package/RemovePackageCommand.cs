using System;
using OpenWrap.Commands.Wrap;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.Package
{
    public class RemovePackageCommand : CommandBase<PackageViewModel>
    {
        protected override void Execute(PackageViewModel parameter)
        {
            RemoveWrapCommand removeWrapCommand = new RemoveWrapCommand
            {
                Name = parameter.Name
            };

            _commandOutput = removeWrapCommand.Execute();
        }
    }
}
