using System.Collections.Generic;

namespace OpenWrap.Commands.Cli
{
    public class AmbiguousInputName : Error
    {
        public AmbiguousInputName(string input, IEnumerable<string> potentialInputs)
            : base("The input '{0}' was ambiguous. Possible inputs: {1}", input, potentialInputs.JoinString(", "))
        {
            Input = input;
            PotentialInputs = potentialInputs;
        }

        public string Input { get; private set; }
        public IEnumerable<string> PotentialInputs { get; private set; }
    }
}