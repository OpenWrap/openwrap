using System;

namespace OpenWrap.Commands
{
    public class InputParsingError : Error
    {
        public InputParsingError(string inputName, string attemptedValue)
            : base("Cannot assign '{0}' to input '{1}'. Make sure it is of the correct type and within the allowed range.", attemptedValue, inputName)
        {
            InputName = inputName;
            AttemptedValue = attemptedValue;
        }

        public string InputName { get; set; }
        public string AttemptedValue { get; set; }
    }
}