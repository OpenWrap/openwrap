using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenWrap.Commands
{
    public class AmbiguousInputName : ICommandOutput
    {
        readonly string _input;
        public IEnumerable<string> PotentialInputs { get; private set; }

        public AmbiguousInputName(string input, IEnumerable<string> potentialInputs)
        {
            _input = input;
            PotentialInputs = potentialInputs;
        }

        CommandResultType ICommandOutput.Type
        {
            get { return CommandResultType.Error; }
        }

        public override string ToString()
        {
            return string.Format("The input '{0}' was ambiguous. Possible inputs: {1}", _input, PotentialInputs.JoinString(", "));
        }
    }
}