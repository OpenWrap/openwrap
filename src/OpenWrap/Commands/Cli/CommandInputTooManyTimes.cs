namespace OpenWrap.Commands.Cli
{
    public class CommandInputTooManyTimes : Error
    {
        public CommandInputTooManyTimes(string inputName)
                : base("Cannot assing input '{0}' because it has been specified multiple times. To provide multiple values to an input, separate entries with a coma, such as '-input value1, value2'.", inputName)
        {
            InputName = inputName;
        }

        public string InputName { get; set; }
    }
}