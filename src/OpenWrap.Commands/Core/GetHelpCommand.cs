using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Services;

namespace OpenWrap.Commands.Core
{
    [Command(Verb = "get", Noun = "help", IsDefault = true)]
    public class GetHelpCommand : ICommand
    {
        [CommandInput(Position = 0)]
        public string CommandName { get; set; }

        protected ICommandRepository CommandRepository
        {
            get { return ServiceLocator.GetService<ICommandRepository>(); }
        }

        public IEnumerable<ICommandOutput> Execute()
        {
            return string.IsNullOrEmpty(CommandName) ? ListAllCommands(CommandRepository.Where(x => x.Visible)) : ListCommand();
        }

        IEnumerable<ICommandOutput> CommandDescription(ICommandDescriptor matchingCommand)
        {
            yield return new CommandDescriptionOutput(matchingCommand);
        }

        IEnumerable<ICommandOutput> CommandNotFound()
        {
            yield return new Error("Command '{0}' not found.", CommandName);
        }

        IEnumerable<ICommandOutput> ListAllCommands(IEnumerable<ICommandDescriptor> commandRepository)
        {
            yield return new Info(Environment.NewLine + "List of available commands" + Environment.NewLine + "--------------------------");
            yield return new Info(Environment.NewLine + "Usage:" + Environment.NewLine + "  o {{verb}}-{{noun}} <command-arguments>" + Environment.NewLine);
            var groups = commandRepository
                .Where(x => string.IsNullOrEmpty(x.Noun) == false && string.IsNullOrEmpty(x.Verb) == false)
                .GroupBy(r => r.Noun)
                .OrderBy(g => g.Key)
                .Select(g => new CommandGroupResult(g.Key, g));
            foreach (var group in groups)
            {
                yield return group;
            }

            yield return new Info(Environment.NewLine + "For detailed help on a particular command, run:" + Environment.NewLine + "  o get-help {{verb}}-{{noun}}");
        }

        IEnumerable<ICommandOutput> ListCommand()
        {
            var matchingCommands = CommandRepository.Where(x => (x.Verb + "-" + x.Noun).ContainsNoCase(CommandName)).OrderBy(d => d.Noun).ThenBy(d => d.Verb).ToList();
            if (matchingCommands.Count == 0)
                return CommandNotFound();
            if (matchingCommands.Count > 1)
                return MultipleCommands(matchingCommands);

            return CommandDescription(matchingCommands[0]);
        }

        IEnumerable<ICommandOutput> MultipleCommands(List<ICommandDescriptor> matchingCommands)
        {
            return ListAllCommands(matchingCommands);
        }
    }
}