using System.Collections.Generic;
using OpenWrap.Commands.Cli.Parser;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_parsing
{
    public abstract class input_parser : context
    {
        protected IEnumerable<Input> Result { get; set; }

        protected void when_parsing(string input)
        {
            Result = InputParser.Parse(input);
        }
    }
}