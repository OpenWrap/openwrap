namespace OpenWrap.Commands
{
    public class UnknownCommand : Error
    {
        public UnknownCommand(string commandName)
        {
            CommandName = commandName;
        }

        public string CommandName { get; set; }

        public override string ToString()
        {
            return string.Format(Strings.CMD_UNKNOWN, CommandName);
        }
    }
}