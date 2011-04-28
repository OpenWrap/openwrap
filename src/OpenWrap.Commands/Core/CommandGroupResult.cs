using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Core
{
    public class CommandGroupResult : Success
    {
        readonly string _noun;
        readonly IEnumerable<ICommandDescriptor> _commands;

        public CommandGroupResult(string noun, IEnumerable<ICommandDescriptor> commands)
        {
            _noun = noun;
            _commands = commands.Where(x=>x.IsDefault).Concat(commands.Where(x=>x.IsDefault == false).OrderBy(c => c.Verb));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(_noun);
            foreach (var command in _commands)
            {
                builder.Append(command.IsDefault ? " > " : "   ").Append(command.Verb).Append(": ").AppendLine(command.Description);
            }
            return builder.ToString();
        }
    }
}
