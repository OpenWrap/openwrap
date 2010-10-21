using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Core
{
    public class CommandGroupResult : Success
    {
        readonly string _noun;
        readonly ICommandDescriptor[] _commands;

        public CommandGroupResult(string noun, IEnumerable<ICommandDescriptor> commands)
        {
            _noun = noun;
            _commands = commands.OrderBy(c => c.Verb).ToArray();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(_noun);
            foreach (var command in _commands)
            {
                builder.Append("  ").Append(command.Verb).Append(": ").AppendLine(command.Description);
            }
            return builder.ToString();
        }
    }
}
