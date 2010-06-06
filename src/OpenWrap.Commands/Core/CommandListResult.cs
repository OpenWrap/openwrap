namespace OpenWrap.Commands.Core
{
    public class CommandListResult : Success
    {
        readonly ICommandDescriptor _command;

        public CommandListResult(ICommandDescriptor repository)
        {
            _command = repository;
        }
        public override string ToString()
        {
            return _command.Noun + " " + _command.Verb;
        }
    }
}