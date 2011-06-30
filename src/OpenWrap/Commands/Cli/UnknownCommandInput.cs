namespace OpenWrap.Commands.Cli
{
    public class UnknownCommandInput : Error
    {
        public UnknownCommandInput(string inputName) : base("An input cannot be found that matches the name '{0}'.", inputName)
        {
            InputName = inputName;
        }

        public string InputName { get; set; }
    }
}