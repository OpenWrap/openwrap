using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.Services;

namespace OpenWrap.Commands.Core
{
    [Command(Verb="get", Noun="help")]
    public class HelpCommand : ICommand
    {
        public IEnumerable<ICommandResult> Execute()
        {
            yield return new Result("List of commands");
            foreach (var command in WrapServices.GetService<ICommandRepository>())
            {
                yield return new CommandListResult(command);
            }
        }
    }
}
