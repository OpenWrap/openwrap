using System;
using System.Linq;
using OpenWrap.Commands.Wrap;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.Package
{
    public class AddPackageCommand : CommandBase<PackageViewModel>
    {
        protected override void Execute(PackageViewModel parameter)
        {
            AddWrapCommand addWrapCommand = new AddWrapCommand
            {
                    Name = parameter.Name
            };

            CommandOutput = addWrapCommand.Execute().ToList();
        }
    }
}
