using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.Windows.Framework.Messaging
{
    public static class CommandHelper
    {
        public static void ExecuteAndSend(ICommand command)
        {
            IEnumerable<ICommandOutput> commandOutput = command.Execute().ToList();
            Messenger.Default.Send("CommandOutput", commandOutput);
        }
    }
}
