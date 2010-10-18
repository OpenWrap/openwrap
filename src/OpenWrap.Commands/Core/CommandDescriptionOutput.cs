using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Core
{
    public class CommandDescriptionOutput : GenericMessage
    {
        public const string TEMPLATE =
                "COMMAND\r\n" +
                "\t{0}\r\n\r\n" +
                "DESCRIPTION\r\n" +
                "\t{1}\r\n\r\n" +
                "USAGE\r\n" +
                "\t{2}\r\n\r\n" +
                "PARAMETERS\r\n{3}";
        public ICommandDescriptor Command { get; set; }

        public CommandDescriptionOutput(ICommandDescriptor command) : base(null)
        {
            Command = command;
            CommandName = string.Format("{0}-{1}", command.Verb, command.Noun);
            Description = command.Description;
            UsageLine = CreateUsageLine(command);
            Parameters = CreateParameters(command);
        }

        string CreateParameters(ICommandDescriptor command)
        {
            return PositionalParameters(command)
                    .Concat(NonPositionalParameters(command))
                    .Select(x => CreateInputDescription(x))
                    .Join("\r\n\r\n");
        }

        string CreateInputDescription(ICommandInputDescriptor input)
        {
            return string.Format("\t-{0} <{1}>\r\n\t\t{2}", input.Name, input.Type.Name, input.Description);
        }

        protected string CommandName { get; set; }

        string CreateUsageLine(ICommandDescriptor command)
        {
            var positionedParameters = CreatePositionedParameters(command);
            var unpositionedParameters = CreateUnpositionedParameters(command);
            return new[] { CommandName, positionedParameters, unpositionedParameters }.NotNullOrEmpty().Join(" ");
        }

        string CreateUnpositionedParameters(ICommandDescriptor command)
        {
            return NonPositionalParameters(command)
                    .Select(CreateUnpositionedParameter)
                    .Join(" ");
        }

        IEnumerable<ICommandInputDescriptor> NonPositionalParameters(ICommandDescriptor command)
        {
            return command.Inputs
                    .Select(x=>x.Value)
                    .Where(x => x.Position == null && x.IsRequired)
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .Concat(
                        command.Inputs.Select(x => x.Value)
                                      .Where(x => x.Position == null && x.IsRequired==false)
                                      .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    );
        }

        string CreateUnpositionedParameter(ICommandInputDescriptor command)
        {
            return string.Format(!command.IsRequired ? "[-{0} <{1}>]" : "-{0} <{1}>", command.Name, command.Type.Name);
        }

        string CreatePositionedParameters(ICommandDescriptor command)
        {
            return PositionalParameters(command)
                    .Select(CreatePositionedParameter)
                    .Join(" ");
        }

        IOrderedEnumerable<ICommandInputDescriptor> PositionalParameters(ICommandDescriptor command)
        {
            return command.Inputs.Select(x=>x.Value)
                    .Where(x => x.Position != null)
                    .OrderBy(x => x.Position);
        }

        string CreatePositionedParameter(ICommandInputDescriptor x)
        {
            return string.Format(!x.IsRequired ? "[[-{0}] <{1}>]" : "[-{0}] <{1}>", x.Name, x.Type.Name);
        }
        
        public string UsageLine { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format(TEMPLATE, CommandName, Description, UsageLine, Parameters);
        }

        protected string Parameters { get; set; }
    }
}