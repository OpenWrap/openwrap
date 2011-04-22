using OpenWrap.Commands;

namespace Tests.Commands.cecil_commands
{
    [Command]
    public class CommandWithOptionalInput : TestableCommand
    {
        string _towards;
        public bool TowardsAssigned;

        [CommandInput]
        public string Towards
        {
            get { return _towards; }
            set { _towards = value;
                TowardsAssigned = true;
            }
        }
    }
}