using OpenWrap.Commands;

namespace Tests.Commands.cecil_commands
{
    [Command]
    public class CommandWithOptionalInputValue : TestableCommand
    {
        string _doIt;
        public bool DoItAssigned;

        [CommandInput(IsValueRequired = false)]
        public string DoIt
        {
            get { return _doIt; }
            set { _doIt = value;
                DoItAssigned = true;
            }
        }
    }
}