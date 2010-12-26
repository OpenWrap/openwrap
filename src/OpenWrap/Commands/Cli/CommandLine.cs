using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public class CommandLine
    {
        public CommandLine(string noun, string verb, IEnumerable<string> arguments)
        {
            Noun = noun;
            Verb = verb;
            Arguments = arguments.ToArray();
        }

        public IEnumerable<string> Arguments { get; private set; }

        public string Noun { get; private set; }
        public string Verb { get; private set; }
    }
}