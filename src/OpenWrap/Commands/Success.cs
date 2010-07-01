namespace OpenWrap.Commands
{
    public class Success : ICommandResult
    {
        bool ICommandResult.Success
        {
            get { return true; }
        }
        public override string ToString()
        {
            return "The command executed successfully.";
        }
        public ICommand Command { get; set; }
    }
}