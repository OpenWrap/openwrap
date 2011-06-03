using System.Collections.Generic;

namespace OpenWrap.Commands.Cli.Locators
{
    public static class CommandDescriptorComparer
    {
        public static readonly IEqualityComparer<ICommandDescriptor> VerbNoun = new CommandVerbNounComparer();

        class CommandVerbNounComparer : IEqualityComparer<ICommandDescriptor>
        {
            public bool Equals(ICommandDescriptor x, ICommandDescriptor y)
            {
                if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
                return x.Noun.EqualsNoCase(y.Noun) && x.Verb.EqualsNoCase(y.Verb);
            }

            public int GetHashCode(ICommandDescriptor obj)
            {
                return (obj.Verb + "-" + obj.Noun).GetHashCode();
            }
        }
    }
}