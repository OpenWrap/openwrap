using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Cli
{
    public class CommandRunner
    {
        public IEnumerable<ICommandOutput> Run(ICommandDescriptor command, string line)
        {
            var commandInstance = command.Create();
            var parsedInput = new InputParser().Parse(line).ToList();
            var unassignedInputs = new List<Input>();
            var assignedInputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach(var input in parsedInput)
            {
                var inputName = input.Name;
                if (!string.IsNullOrEmpty(inputName))
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
                    if (!command.Inputs.TryGetValue(inputName, out assigner))
                    {
                        yield return new UnknownCommandInput(inputName);
                        yield break;
                    }
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
            if (unassignedInputs.Count > 0)
            {
                foreach (var positioned in command.Inputs
                        .Where(x => !assignedInputs.Contains(x.Key))
                        .Select(x => x.Value)
                        .OrderBy(x => x.Position))
                {
                    if (unassignedInputs.Count == 0) break;

                    if (!AssignValue(unassignedInputs[0], commandInstance, positioned))
                    {
                        yield return new InputParsingError(positioned.Name, GetLinearValue(unassignedInputs[0]));
yield
                        break;
                    }
                    assignedInputs.Add(unassignedInputs[0].Name);
                    unassignedInputs.RemoveAt(0);
                }
            }
            var missingInputs = command.Inputs.Where(x => x.Value.IsRequired && !assignedInputs.Contains(x.Key)).Select(x=>x.Value);
            if (missingInputs.Any())
            {
                yield return new MissingInput(missingInputs);
                yield break;
            }
            foreach (var output in commandInstance.Execute())
                yield return output;
        }

        string GetLinearValue(Input input)
        {
            return input is SingleValueInput ? ((SingleValueInput)input).Value : ((MultiValueInput)input).Values.Join(", ");
        }

        bool AssignValue(Input input, ICommand command, ICommandInputDescriptor descriptor)
        {
            var s = input as SingleValueInput;
            if (s != null)
                return descriptor.TrySetValue(command, new[] { s.Value });

            var m = input as MultiValueInput;
            if (m != null)
                return descriptor.TrySetValue(command, m.Values);
            throw new InvalidOperationException();
        }
    }

    public class CommandInputTooManyTimes : Error
    {
        public CommandInputTooManyTimes(string inputName)
            : base("Cannot assing input '{0}' because it has been specified multiple times. To provide multiple values to an input, separate entries with a coma, such as '-input value1, value2'.", inputName)
        {
            InputName = inputName;
        }

        public string InputName { get; set; }
    }
}