using OpenWrap.Commands;

namespace OpenWrap.Windows
{
    public class VerbSlice
    {
        public VerbSlice(ICommandDescriptor commandDescriptor)
        {
            Verb = commandDescriptor.Verb;
            this.Command = commandDescriptor;
        }

        public ICommandDescriptor Command { get; set; }

        public string Verb { get; set; }
    }
}