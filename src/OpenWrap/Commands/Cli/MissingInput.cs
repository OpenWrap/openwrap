using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public class MissingInput : Error
    {
        public MissingInput(IEnumerable<ICommandInputDescriptor> missingInputs)
            : base("Missing values for the following command inputs: {0}", missingInputs.Aggregate(string.Empty, (s, c) => s + (c.Name + " ")))
        {
            MissingInputs = missingInputs;
        }

        public IEnumerable<ICommandInputDescriptor> MissingInputs { get; private set; }
    }
}