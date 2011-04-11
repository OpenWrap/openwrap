namespace OpenWrap.Commands.Cli
{
    public abstract class CommandLineHandler
    {
        protected ICommandRepository Commands { get; private set; }

        public CommandLineHandler(ICommandRepository commands)
        {
            Commands = commands;
        }
    }
}