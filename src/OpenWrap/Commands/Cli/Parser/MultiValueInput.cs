using System.Collections.Generic;

namespace OpenWrap.Commands.Cli
{
    public class MultiValueInput : Input
    {
        public ICollection<string> Values { get; set; }
    }
}