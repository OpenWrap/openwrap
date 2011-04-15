using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands
{
    public class MissingInput : Error
    {
        public IEnumerable<ICommandInputDescriptor> MissingInputs { get; private set; }

        public MissingInput(IEnumerable<ICommandInputDescriptor> missingInputs)
        {
            MissingInputs = missingInputs;
        }

        public override string ToString()
        {
            return "Missing values for the following command inputs: " + MissingInputs.Aggregate(string.Empty, (s, c) => s += c.Name + " ");
        }
    }
}