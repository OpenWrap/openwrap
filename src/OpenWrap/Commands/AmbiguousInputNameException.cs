using System;
using System.Runtime.Serialization;

namespace OpenWrap.Commands
{
    [Serializable]
    public class AmbiguousInputNameException : Exception, ICommandOutput
    {
        readonly string _input;
        readonly string[] _potentialInputs;

        public AmbiguousInputNameException(string input,string[] potentialInputs) : base("Ambiguous input name.")
        {
            _input = input;
            _potentialInputs = potentialInputs;
        }

        protected AmbiguousInputNameException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
        {
        }

        bool ICommandOutput.Success
        {
            get { return false; }
        }

        ICommand ICommandOutput.Source
        {
            get { return null; }
        }

        CommandResultType ICommandOutput.Type
        {
            get { return CommandResultType.Error; }
        }
        public override string ToString()
        {
            return string.Format("The input '{0}' was ambiguous. Possible inputs: {1}", _input, string.Join(", ", _potentialInputs));
        }
    }
}