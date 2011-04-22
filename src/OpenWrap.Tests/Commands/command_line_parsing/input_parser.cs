using System.Collections.Generic;
using OpenWrap.Commands.Cli;
using OpenWrap.Commands.Cli.Parser;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing.contexts
{
    public abstract class input_parser : context
    {
        public void when_parsing(string input)
        {
            Result = new InputParser().Parse(input);
        }

        protected IEnumerable<Input> Result { get; set; }
    }
}