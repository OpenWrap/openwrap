using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb="manageui", DisplayName="Manage wraps", Namespace="wrap")]
    public class ManageWrapCommand : ICommand
    {
        public ICommandResult Execute()
        {
            return new Success();
        }
    }

    //public class ManageWrapCommand : MethodBasedCommand
    //{
    //    public ManageWrapCommand()
    //    {
            
    //    }
    //}
}
