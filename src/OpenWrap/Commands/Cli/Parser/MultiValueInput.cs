using System.Collections.Generic;

namespace OpenWrap.Commands.Cli.Parser
{
    public class MultiValueInput : Input
    {
        public ICollection<string> Values { get; set; }
    }
}