using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public class CommandLineParser
    {
        public Result Parse(IEnumerable<string> strings)
        {
            if (strings == null) return new NotEnoughArgumentsFailure();

            var array = strings.ToArray();

            if (array.Length < 1) return new NotEnoughArgumentsFailure();

            var dashIndex = array[0].IndexOf('-');
            if (dashIndex > 0)
            {
                return ParseVerbNounFormat(array, dashIndex);
            }
            if (array.Length < 2) return new NotEnoughArgumentsFailure();

            return ParseNounVerbFormat(array);
        }

        static Result ParseNounVerbFormat(string[] array)
        {
            return new Success(new CommandLine(
                                       array[0],
                                       array[1],
                                       array.Skip(2)
                                       ));
        }

        static Result ParseVerbNounFormat(string[] array, int dashIndex)
        {
            return new Success(new CommandLine(
                                       array[0].Substring(dashIndex + 1),
                                       array[0].Substring(0, dashIndex),
                                       array.Skip(1)
                                       ));
        }

        public class NotEnoughArgumentsFailure : Result
        {
        }

        public abstract class Result
        {
        }

        public class Success : Result
        {
            public Success(CommandLine commandLine)
            {
                CommandLine = commandLine;
            }

            public CommandLine CommandLine { get; private set; }
        }
    }
}