using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Client;
using OpenWrap.Commands.Cli.Locators;

namespace OpenWrap.Commands.Cli
{
    // NOTE: This class is temporary, don't rely on it, it will change
    public class ConsoleCommandExecutor
    {
        readonly IEnumerable<ICommandLocator> _handlers;
        readonly IEventHub _eventHub;
        readonly ICommandOutputFormatter _formatter;

        public ConsoleCommandExecutor(IEnumerable<ICommandLocator> handlers, IEventHub eventHub, ICommandOutputFormatter formatter)
        {
            _handlers = handlers;
            _eventHub = eventHub;
            _formatter = formatter;
        }

        public int Execute(string commandLine, IEnumerable<string> optionalInputs)
        {
            commandLine = commandLine.Trim();
            if (commandLine == string.Empty) return 0;
            string commandParameters = commandLine;
            var command = _handlers.Select(x => x.Execute(ref commandParameters)).Where(x => x != null).FirstOrDefault();
            if (command == null)
            {
                var sp = commandLine.IndexOf(" ");

                _formatter.Render(new Error("The term '{0}' is not a recognized command or alias. Check the spelling or enter 'get-help' to get a list of available commands.",
                           sp != -1 ? commandLine.Substring(0, sp) : commandLine));
                return -10;
            }
            int returnCode = 0;
            var commandLineRunner = new CommandLineRunner { OptionalInputs = optionalInputs };
            using (DownloadNotifier())
            using (_eventHub.Subscribe<ICommandOutput>(_formatter.Render))
            {
                foreach (var output in commandLineRunner.Run(command, commandParameters))
                {
                    _eventHub.Publish(output);
                    if (output.Type == CommandResultType.Error)
                    {
                        returnCode = -50;
                    }
                }

            }
            return returnCode;
        }
        IDisposable DownloadNotifier()
        {
            // like the rest of this class, this is monstruous and needs a redesign.

            HttpNotifier.Default.NewRequest += NewRequest;

            return new ActionOnDispose(() => HttpNotifier.Default.NewRequest -= NewRequest);
        }

        void NewRequest(IRequestProgress obj)
        {

            Action<TransferProgress> onUpload = (progress) =>
            {
                ConsoleEx.WriteProgress(
                    obj.RequestUri.ToString(),
                    FormatUri(obj),
                    progress.Finished ? 100 : ToPercent(progress));
            };
            Action<TransferProgress> onDownload = (progress) =>
            {
                ConsoleEx.WriteProgress(
                    obj.RequestUri.ToString(),
                    FormatUri(obj),
                    progress.Finished ? 201 : 100 + ToPercent(progress));
            };

            obj.Upload += onUpload;
            obj.Download += onDownload;
        }

        int ToPercent(TransferProgress progress)
        {
            return (int)((progress.Current * 100) / progress.Total);
        }

        static string FormatUri(IRequestProgress obj)
        {
            return obj.RequestUri.ToString();
        }
    }
}