using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.Windows.Framework.Messaging
{
    public static class CommandHelper
    {
        /// <summary>
        /// Execute an OpenWrap.Commands.ICommand and send a notification of the resulting output
        /// </summary>
        /// <param name="command">the OpenWrap command to execute</param>
        public static void ExecuteAndSend(ICommand command)
        {
            IEnumerable<ICommandOutput> commandOutput = command.Execute().ToList();
            Messenger.Default.Send(MessageNames.CommandOutput, commandOutput);
        }
    }
}
