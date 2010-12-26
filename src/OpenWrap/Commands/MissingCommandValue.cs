using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands
{
    public class MissingCommandValue : Error
    {
        readonly List<ICommandInputDescriptor> _missingInputs;

        public MissingCommandValue(List<ICommandInputDescriptor> missingInputs)
        {
            _missingInputs = missingInputs;
        }

        public override string ToString()
        {
            return "Missing values for the following command inputs: " + _missingInputs.Aggregate(string.Empty, (s, c) => s += c.Name + " ");
        }
    }
}