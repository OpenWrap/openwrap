using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Wrap.Build.Services;
using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Console;

namespace OpenWrap.Commands.Core
{
    [Command(Verb="get", Namespace="help")]
    public class HelpCommand : ICommand
    {
        public ICommandResult Execute()
        {
            return new CommandListResult(WrapServices.GetService<ICommandRepository>());
        }
    }

    public class CommandListResult : Success
    {
        readonly ICommandRepository _repository;

        public CommandListResult(ICommandRepository repository)
        {
            _repository = repository;
        }
        public override string ToString()
        {
            return _repository.Aggregate("List of commands",
                                         (s, c) => s += "\r\n- " + c.Namespace + " " + c.Verb);
        }
    }
}
