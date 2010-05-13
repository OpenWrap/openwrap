using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Wrap.Commands;

namespace OpenRasta.Wrap.Console
{
    public class CommandLineProcessor
    {
        readonly CommandRepository _commands;

        public CommandLineProcessor(CommandRepository commands)
        {
            _commands = commands;
        }

        public ICommandResult Execute(string[] strings)
        {
            if (strings == null || strings.Length < 2)
                return new NotEnoughParameters();

            var matchingNamespaces = _commands.Namespaces.Where(x => x.StartsWith(strings[0], StringComparison.OrdinalIgnoreCase)).ToList();
            if (matchingNamespaces.Count != 1)
                return new NamesapceNotFound(matchingNamespaces);
            var ns = matchingNamespaces[0];

            var matchingVerbs = _commands.Verbs.Where(x => x.StartsWith(strings[1], StringComparison.OrdinalIgnoreCase)).ToList();

            if (matchingVerbs.Count != 1)
                return new UnknownCommand(strings[1], matchingVerbs);

            var verb = matchingVerbs[0];

            var command = _commands.Get(ns, verb);

            var commandInputValues = ParseCommandInputs(strings.Skip(2)).ToLookup(x => x.Key, x => x.Value);

            var unnamedCommandInputValues = commandInputValues[null].ToList();

            var namedInputs = (from namedValues in commandInputValues
                         where namedValues.Key != null
                         let value = namedValues.LastOrDefault()
                         let commandInput = command.Inputs.ContainsKey(namedValues.Key) ? command.Inputs[namedValues.Key] : null
                         let parsedValue = commandInput != null ? commandInput.ValidateValue(value) : null
                         select new ParsedInput
                         {
                             InputName = namedValues.Key,
                             RawValue = value,
                             Input = commandInput,
                             ParsedValue = parsedValue
                         }).ToList();

            var namedInputsNameNotFound = namedInputs.FirstOrDefault(x => x.Input == null);
            if (namedInputsNameNotFound != null) return new UnknownCommandInput(namedInputsNameNotFound.InputName);

            var namedInputsValueNotParsed = namedInputs.FirstOrDefault(x => x.RawValue == null);
            if (namedInputsValueNotParsed != null) return new InvalidCommandValue(namedInputsValueNotParsed.InputName, namedInputsValueNotParsed.RawValue);

            var inputNamesAlreadyFilled = namedInputs.Select(x => x.InputName).ToList();
            // now got a clean set of input names that pass. Now on to the unnamed ones.

            var unfullfilledCommands = (from input in command.Inputs
                                        where !inputNamesAlreadyFilled.Contains(input.Key, StringComparer.OrdinalIgnoreCase)
                                              && input.Value.Position >= 0
                                        orderby input.Value.Position ascending
                                        select input.Value).ToList();
            if (unnamedCommandInputValues.Count > unfullfilledCommands.Count) return new InvalidCommandValue(unnamedCommandInputValues);

            var unnamedCommandValues = (from unnamedValue in unnamedCommandInputValues
                                        let input = unfullfilledCommands[unnamedCommandInputValues.IndexOf(unnamedValue)]
                                        let commandValue = input.ValidateValue(unnamedValue)
                                        select new ParsedInput()
                                        {
                                            InputName = null,
                                            RawValue = unnamedValue,
                                            Input = input,
                                            ParsedValue = commandValue
                                        }).ToList();


            var unnamedFailed = unnamedCommandValues.Where(x => x.ParsedValue == null).ToList();
            if (unnamedFailed.Count > 0) return new InvalidCommandValue(unnamedCommandInputValues);

            // all clear, assign and run
            var commandInstance = command.Create();
            foreach (var namedInput in namedInputs.Concat(unnamedCommandValues))
                namedInput.Input.SetValue(commandInstance, namedInput.ParsedValue);

            return commandInstance.Execute();
        }
        class ParsedInput
        {
            public string InputName;
            public string RawValue;
            public ICommandInputDescriptor Input;
            public object ParsedValue;
        }
        IEnumerable<KeyValuePair<string, string>> ParseCommandInputs(IEnumerable<string> strings)
        {
            string key = null;
            foreach (var component in strings)
            {
                if (component.StartsWith("-"))
                {
                    if (key != null)
                        yield return new KeyValuePair<string, string>(key, null);
                    key = component.Substring(1);
                    continue;
                }
                if (key != null)
                {
                    yield return new KeyValuePair<string, string>(key, component);
                    key = null;
                    continue;
                }
                yield return new KeyValuePair<string, string>(null, component);
            }
        }
    }
}