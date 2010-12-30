using System.Collections.Generic;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows.NounVerb
{
    public class NounSlice : ViewModelBase
    {
        public NounSlice(string noun, IEnumerable<VerbSlice> commandDescriptors)
        {
            Noun = noun;
            Commands = commandDescriptors;
        }

        public IEnumerable<VerbSlice> Commands { get; set; }

        public string Noun { get; set; }
    }
}