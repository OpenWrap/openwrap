using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Commands;

namespace OpenWrap.Commands.Core
{
    [Command(Verb="get", Namespace="help")]
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
    public class Result : ICommandResult
    {
        readonly string _value;

        public Result(object value)
        {
            _value = value.ToString();
            Success = true;
        }
        public Result(string str, params object[] parameters)
        {
            _value = string.Format(str, parameters);
            Success = true;
        }
        public bool Success
        {
            get; set;
        }

        public ICommand Command
        {
            get { throw new NotImplementedException(); }
        }
        public override string ToString()
        {
            return _value;
        }
    }

    public class CommandListResult : Success
    {
        readonly ICommandDescriptor _command;

        public CommandListResult(ICommandDescriptor repository)
        {
            _command = repository;
        }
        public override string ToString()
        {
            return _command.Noun + " " + _command.Verb;
        }
    }
}
