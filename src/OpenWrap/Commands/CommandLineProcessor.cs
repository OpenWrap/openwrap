using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public class CommandLineProcessor
    {
        readonly ICommandRepository _commands;

        public CommandLineProcessor(ICommandRepository commands)
        {
            _commands = commands;
        }

        public IEnumerable<ICommandResult> Execute(IEnumerable<string> strings)
        {
            if (strings == null || strings.Count() < 2)
            {
                yield return new NotEnoughParameters();
                yield break;
            }

            var matchingNamespaces = _commands.Namespaces.Where(x => x.StartsWith(strings.ElementAt(0), StringComparison.OrdinalIgnoreCase)).ToList();
            if (matchingNamespaces.Count != 1)
            {yield return new NamesapceNotFound(matchingNamespaces);
                yield break;
            }
            var ns = matchingNamespaces[0];

            var matchingVerbs = _commands.Verbs.Where(x => x.StartsWith(strings.ElementAt(1), StringComparison.OrdinalIgnoreCase)).ToList();

            if (matchingVerbs.Count != 1)
            {yield return new UnknownCommand(strings.ElementAt(1), matchingVerbs);
                yield break;}

            var verb = matchingVerbs[0];

            var command = _commands.Get(ns, verb);

            var commandInputValues = ParseCommandInputs(strings.Skip(2)).ToLookup(x => x.Key, x => x.Value);

            var unnamedCommandInputValues = commandInputValues[null].ToList();

            var assignedNamedInputValues = (from namedValues in commandInputValues
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

            var namedInputsNameNotFound = assignedNamedInputValues.FirstOrDefault(x => x.Input == null);
            if (namedInputsNameNotFound != null)
            {
                yield return new UnknownCommandInput(namedInputsNameNotFound.InputName);
                yield break;
            }

            var namedInputsValueNotParsed = assignedNamedInputValues.FirstOrDefault(x => x.RawValue == null);
            if (namedInputsValueNotParsed != null)
            {
                yield return new InvalidCommandValue(namedInputsValueNotParsed.InputName, namedInputsValueNotParsed.RawValue);
                yield break;
            }

            var inputNamesAlreadyFilled = assignedNamedInputValues.Select(x => x.InputName).ToList();
            // now got a clean set of input names that pass. Now on to the unnamed ones.

            var unfullfilledCommandInputs = (from input in command.Inputs
                                        where !inputNamesAlreadyFilled.Contains(input.Key, StringComparer.OrdinalIgnoreCase)
                                              && input.Value.Position >= 0
                                        orderby input.Value.Position ascending
                                        select input.Value).ToList();

            if (unnamedCommandInputValues.Count > unfullfilledCommandInputs.Count) {yield return new InvalidCommandValue(unnamedCommandInputValues);
                yield break;}

            var assignedUnnamedInputValues = (from unnamedValue in unnamedCommandInputValues
                                        let input = unfullfilledCommandInputs[unnamedCommandInputValues.IndexOf(unnamedValue)]
                                        let commandValue = input.ValidateValue(unnamedValue)
                                        select new ParsedInput()
                                        {
                                            InputName = null,
                                            RawValue = unnamedValue,
                                            Input = input,
                                            ParsedValue = commandValue
                                        }).ToList();


            var unnamedFailed = assignedUnnamedInputValues.Where(x => x.ParsedValue == null).ToList();
            if (unnamedFailed.Count > 0)
            {
                yield return new InvalidCommandValue(unnamedCommandInputValues);
                yield break;
            }

            var allAssignedInputs = assignedNamedInputValues.Concat(assignedUnnamedInputValues);

            var missingInputs = command.Inputs.Select(x => x.Value).Where(x => !allAssignedInputs.Select(i => i.Input).Contains(x) && x.IsRequired).ToList();
            if (missingInputs.Count > 0)
            {
                yield return new MissingCommandValue(missingInputs);
                yield break;
            }

            var missingInputValues = allAssignedInputs.Select(x => x.Input);
            // all clear, assign and run
            var commandInstance = command.Create();
            foreach (var namedInput in allAssignedInputs)
                namedInput.Input.SetValue(commandInstance, namedInput.ParsedValue);
            foreach(var nestedResult in commandInstance.Execute())
                yield return nestedResult;
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