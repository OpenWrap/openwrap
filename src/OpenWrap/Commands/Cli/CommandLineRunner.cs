using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands.Cli.Parser;

namespace OpenWrap.Commands.Cli
{
    public class CommandLineRunner
    {
        public CommandLineRunner()
        {
            OptionalInputs = Enumerable.Empty<string>();
        }

        public ICommand LastCommand { get; private set; }

        public IEnumerable<string> OptionalInputs { get; set; }

        public IEnumerable<ICommandOutput> Run(ICommandDescriptor command, string line)
        {
            var commandInstance = LastCommand = command.Create();
            var parsedInput = InputParser.Parse(line).ToList();
            var unassignedInputs = new List<Input>();
            var assignedInputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var asWildcard = commandInstance as ICommandWithWildcards;

            foreach (var input in parsedInput)
            {
                var inputName = input.Name;
                if (string.IsNullOrEmpty(inputName))
                {
                    unassignedInputs.Add(input);
                }
                else
                {
                    ICommandInputDescriptor assigner;
                    if (!command.Inputs.TryGetValue(inputName, out assigner))
                    {
                        var potentialInputs = inputName.SelectHumps(command.Inputs.Keys).ToList();
                        if (potentialInputs.Count == 1)
                            inputName = potentialInputs.Single();
                        else if (potentialInputs.Count > 1)
                        {
                            yield return new AmbiguousInputName(inputName, potentialInputs);
                            yield break;
                        }
                    }
                    if (assignedInputs.Contains(inputName))
                    {
                        yield return new CommandInputTooManyTimes(inputName);
                        yield break;
                    }

                    bool canIgnore = OptionalInputs.ContainsNoCase(inputName) || asWildcard != null;
                    if (!command.Inputs.TryGetValue(inputName, out assigner) && !canIgnore)
                    {
                        yield return new UnknownCommandInput(inputName);
                        yield break;
                    }

                    if (assigner != null)
                    {
                        if (!AssignValue(input, commandInstance, assigner))
                        {
                            yield return new InputParsingError(inputName, GetLinearValue(input));
                            yield break;
                        }
                        assignedInputs.Add(inputName);
                    }
                    else
                    {
                        unassignedInputs.Add(input);
                    }
                }
            }
            var unnamedUnassignedInputs = unassignedInputs.Where(_ => _.Name == string.Empty).ToList();

            if (unnamedUnassignedInputs.Count > 0)
            {
                foreach (var positioned in command.Inputs
                    .Where(x => x.Value.Position != null && !assignedInputs.Contains(x.Key))
                    .Select(x => x.Value)
                    .OrderBy(x => x.Position))
                {
                    if (unnamedUnassignedInputs.Count == 0) break;

                    var firstUnnamedInput = unnamedUnassignedInputs[0];
                    if (!AssignValue(firstUnnamedInput, commandInstance, positioned))
                    {
                        yield return new InputParsingError(positioned.Name, GetLinearValue(unassignedInputs[0]));
                        yield break;
                    }

                    assignedInputs.Add(positioned.Name);

                    unnamedUnassignedInputs.RemoveAt(0);
                    unassignedInputs.Remove(firstUnnamedInput);
                }
            }
            var missingInputs = command.Inputs.Where(x => x.Value.IsRequired && !assignedInputs.Contains(x.Key)).Select(x => x.Value);
            if (missingInputs.Any())
            {
                yield return new MissingInput(missingInputs);
                yield break;
            }
            if (asWildcard != null && unassignedInputs.Any())
            {
                var wildcards = unassignedInputs.SelectMany(GetValues, (input, value) => new { input.Name, value }).ToLookup(_ => _.Name, _ => _.value);
                asWildcard.Wildcards(wildcards);
            }
            foreach (var output in commandInstance.Execute())
                yield return output;
        }

        static bool AssignValue(Input input, ICommand command, ICommandInputDescriptor descriptor)
        {
            return descriptor.TrySetValue(command, GetValues(input));
        }

        static IEnumerable<string> GetValues(Input input)
        {
            var s = input as SingleValueInput;
            if (s != null)
                return s.Value == string.Empty ? Enumerable.Empty<string>() : new[] { s.Value };

            var m = input as MultiValueInput;
            if (m != null)
                return m.Values;

            throw new InvalidOperationException();
        }

        static string GetLinearValue(Input input)
        {
            return input is SingleValueInput ? ((SingleValueInput)input).Value : ((MultiValueInput)input).Values.JoinString(", ");
        }
    }
}