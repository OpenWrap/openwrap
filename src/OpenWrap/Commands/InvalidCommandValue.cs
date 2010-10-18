using System;
using System.Collections.Generic;

namespace OpenWrap.Commands
{
    public class InvalidCommandValue : Error
    {
        public InvalidCommandValue(List<string> unnamed)
            : base(CreateErrorMessage(unnamed))
        {
        }

        static string CreateErrorMessage(List<string> unnamed)
        {
            return "The following values could not be matched to any optional inputs:\r\n"
                   + unnamed.Join("\r\n");
        }
    }
}