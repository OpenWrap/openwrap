using System;

namespace OpenWrap.Commands.Cli
{
    public class ConsoleCommandOutputFormatter : ICommandOutputFormatter
    {
        static IDisposable ColorFromOutput(ICommandOutput output)
        {
            switch (output.Type)
            {
                case CommandResultType.Error:
                    return ConsoleEx.ColoredText.Red;
                case CommandResultType.Warning:
                    return ConsoleEx.ColoredText.Yellow;
                case CommandResultType.Verbose:
                    return ConsoleEx.ColoredText.Gray;
            }
            return new ActionOnDispose(() => { });
        }

        static void WriteError(string message, params string[] args)
        {
            using (ConsoleEx.ColoredText.Red)
                Console.WriteLine(message, args);
        }

        public void Render(ICommandOutput output)
        {
            using (ColorFromOutput(output))
                Console.WriteLine(output.ToString());
        }
    }
}