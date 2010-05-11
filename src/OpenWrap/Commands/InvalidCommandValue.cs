using System.Collections.Generic;

namespace OpenRasta.Wrap.Console
{
    public class InvalidCommandValue : Error
    {
        public InvalidCommandValue(string inputName, string value)
        {
        }

        public InvalidCommandValue(List<string> unnamed)
        {
        }

        public bool Success
        {
            get { return false; }
        }
    }
}