using OpenWrap.Commands;
using OpenWrap;

namespace OpenWrap.Commands
{
    public class UnknownCommand : Error
    {
        public string CommandName { get; set; }

        public UnknownCommand(string commandName)
        {
            CommandName = commandName;
        }
        public override string ToString()
        {
            return string.Format(Strings.CMD_UNKNOWN, CommandName);
        }
    }
}