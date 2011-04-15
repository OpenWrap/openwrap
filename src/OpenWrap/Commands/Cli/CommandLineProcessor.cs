﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.Commands.Cli
{
    /// <summary>
    ///   I publicly apologize to the procedural nature of this class. It is evil and
    ///   as with all things evil should be redesigned at some point.
    /// </summary>
    public class CommandLineProcessor
    {
        readonly ICommandRepository _commands;

        public CommandLineProcessor(ICommandRepository commands)
        {
            _commands = commands;
        }

        public IEnumerable<ICommandOutput> Execute(IEnumerable<string> strings)
        {
            var parser = new CommandLineParser();
            var parseResult = parser.Parse(strings);
            if (parseResult is CommandLineParser.NotEnoughArgumentsFailure)
            {
                yield return new NotEnoughParameters();
                yield break;
            }

            var commandLine = ((CommandLineParser.Success)parseResult).CommandLine;
            var matchingNouns = _commands.Nouns.Where(x => x.StartsWith(commandLine.Noun, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matchingNouns.Count != 1)
            {
                yield return new NounNotFound(matchingNouns);
                yield break;
            }
            var noun = matchingNouns[0];

            var matchingVerbs = _commands.Verbs.Where(x => x.StartsWith(commandLine.Verb, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matchingVerbs.Count != 1)
            {
                yield return new UnknownCommand(commandLine.Verb);
                yield break;
            }

            var verb = matchingVerbs[0];

            var command = _commands.Get(noun, verb);
            if (command == null)
            {
                yield return new UnknownCommand(verb + "-" + noun);
            }
            var inputsFromCommandLine = ParseInputsFromCommandLine(commandLine.Arguments).ToLookup(x => x.Key, x => x.Value);

            var unnamedCommandInputsFromCommandLine = inputsFromCommandLine[null].ToList();
            List<ParsedInput> assignedNamedInputValues = null;
            Exception exception = null;
            try
            {
                assignedNamedInputValues = (from namedValues in inputsFromCommandLine
                                            where namedValues.Key != null
                                            let value = namedValues.LastOrDefault()
                                            let commandInput = FindCommandInputDescriptor(command, namedValues.Key)
                                            let parsedValue = new object()//commandInput != null ? commandInput.ValidateValue(value) : null
                                            select new ParsedInput
                                            {
                                                    InputName = namedValues.Key,
                                                    RawValue = value,
                                                    Input = commandInput,
                                                    ParsedValue = parsedValue
                                            }).ToList();
            }
            catch (Exception e)
            {
                exception = e;
            }
            if (exception != null)
            {
                yield return new ExceptionError(exception);
                yield break;
            }
            var namedInputsNameNotFound = assignedNamedInputValues.FirstOrDefault(x => x.Input == null);
            if (namedInputsNameNotFound != null)
            {
                yield return new UnknownCommandInput(namedInputsNameNotFound.InputName);
                yield break;
            }

            var namedInputsValueNotParsed = assignedNamedInputValues.FirstOrDefault(x => x.RawValue == null);

            var inputNamesAlreadyFilled = assignedNamedInputValues.Select(x => x.InputName).ToList();

            // now got a clean set of input names that pass. Now on to the unnamed ones.
            var unfullfilledCommandInputs = (from input in command.Inputs
                                             where !inputNamesAlreadyFilled.Contains(input.Key, StringComparer.OrdinalIgnoreCase)
                                                   && input.Value.Position != null && input.Value.Position >= 0
                                             orderby input.Value.Position ascending
                                             select input.Value).ToList();

            if (unnamedCommandInputsFromCommandLine.Count > unfullfilledCommandInputs.Count)
            {
                yield return new InvalidCommandValue(unnamedCommandInputsFromCommandLine);
                yield break;
            }

            var assignedUnnamedInputValues = (from unnamedValue in unnamedCommandInputsFromCommandLine
                                              let input = unfullfilledCommandInputs[unnamedCommandInputsFromCommandLine.IndexOf(unnamedValue)]
                                              let commandValue = new object()//null// input.ValidateValue(unnamedValue)
                                              select new ParsedInput
                                              {
                                                      InputName = null,
                                                      RawValue = unnamedValue,
                                                      Input = input,
                                                      ParsedValue = commandValue
                                              }).ToList();


            var unnamedFailed = assignedUnnamedInputValues.Where(x => x.ParsedValue == null).ToList();
            if (unnamedFailed.Count > 0)
            {
                yield return new InvalidCommandValue(unnamedCommandInputsFromCommandLine);
                yield break;
            }

            var allAssignedInputs = assignedNamedInputValues.Concat(assignedUnnamedInputValues);

            allAssignedInputs = TryAssignSwitchParameters(allAssignedInputs, command.Inputs);

            var missingRequiredInputs = command.Inputs.Select(x => x.Value)
                    .Where(x => !allAssignedInputs.Select(i => i.Input).Contains(x) && x.IsRequired)
                    .ToList();
            if (missingRequiredInputs.Count > 0)
            {
                yield return new MissingInput(missingRequiredInputs);
                yield break;
            }

            var missingInputValues = allAssignedInputs.Select(x => x.Input);
            // all clear, assign and run
            var commandInstance = command.Create();
            foreach (var namedInput in allAssignedInputs)
                namedInput.Input.TrySetValue(commandInstance, new[]{namedInput.ParsedValue.ToString()});
            var enumerator = commandInstance.Execute().GetEnumerator();
            MoveNextResult result;
            do
            {
                ICommandOutput msg;
                Exception error;
                result = enumerator.TryMoveNext(out msg, out error);
                if (result == MoveNextResult.Moved)
                    yield return msg;
                else if (result == MoveNextResult.End)
                    yield break;
                else if (result == MoveNextResult.Error)
                    yield return new ExceptionError(error);
            } while (result == MoveNextResult.Moved);
        }

        ICommandInputDescriptor FindCommandInputDescriptor(ICommandDescriptor command, string name)
        {
            ICommandInputDescriptor descriptor;
            // Try to find by full name match first.
            if (command.Inputs.TryGetValue(name, out descriptor))
            {
                return descriptor;
            }
            var potentialInputs = (from input in command.Inputs
                                   where name.MatchesHumps(input.Key)
                                   select input.Value).ToList();

            if (potentialInputs.Count > 1)
                throw new NotSupportedException();// return new AmbiguousInputName(name, potentialInputs.Select(x => x.Name).ToArray());

            return potentialInputs.SingleOrDefault();
        }


        IEnumerable<KeyValuePair<string, string>> ParseInputsFromCommandLine(IEnumerable<string> strings)
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
            if (key != null)
                yield return new KeyValuePair<string, string>(key, null);
        }

        IEnumerable<ParsedInput> TryAssignSwitchParameters(IEnumerable<ParsedInput> allAssignedInputs, IDictionary<string, ICommandInputDescriptor> inputs)
        {
            return allAssignedInputs;
        }

        class ParsedInput
        {
            public ICommandInputDescriptor Input;
            public string InputName;
            public object ParsedValue;
            public string RawValue;
        }
    }
}