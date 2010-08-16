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
            return string.Format("{0} {1}: {2}",_command.Noun, _command.Verb, _command.Description);
        }
    }
}