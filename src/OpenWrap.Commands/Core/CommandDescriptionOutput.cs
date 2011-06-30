using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.Commands.Core
{
    public class CommandDescriptionOutput : AbstractOutput
    {
        public const string TEMPLATE =
            "COMMAND\r\n" +
            "\t{0}\r\n\r\n" +
            "DESCRIPTION\r\n" +
            "\t{1}\r\n\r\n" +
            "USAGE\r\n" +
            "\t{2}\r\n\r\n" +
            "PARAMETERS\r\n{3}";

        public CommandDescriptionOutput(ICommandDescriptor command) : base(null)
        {
            Command = command;
            CommandName = string.Format("{0}-{1}", command.Verb, command.Noun);
            Description = command.Description;
            UsageLine = CreateUsageLine(command);
            Parameters = CreateParameters(command);
        }

        public ICommandDescriptor Command { get; set; }

        public string UsageLine { get; private set; }
        protected string CommandName { get; set; }
        protected string Parameters { get; set; }

        string Description { get; set; }

        public override string ToString()
        {
            return string.Format(TEMPLATE, CommandName, Description, UsageLine, Parameters);
        }

        static string CreateInputDescription(ICommandInputDescriptor input)
        {
            return string.Format("\t-{0} <{1}>\r\n\t\t{2}", input.Name, input.Type, input.Description);
        }

        static string CreatePositionedParameter(ICommandInputDescriptor x)
        {
            string format;
            if (x.IsRequired && x.IsValueRequired)
                format = "[-{0}] <{1}>";
            else if (x.IsRequired)
                format = "-{0} [<{1}>]";
            else if (x.IsValueRequired)
                format = "[[-{0}] <{1}>]";
            else
                format = "(-{0} [<{1}>] | <{1}>)";
            return string.Format(format, x.Name, x.Type);
        }

        static string CreateUnpositionedParameter(ICommandInputDescriptor command)
        {
            string format = command.IsValueRequired ? "<{1}>" : "[<{1}>]";
            format = "-{0} " + format;
            if (!command.IsRequired)
                format = "[" + format + "]";
            return string.Format(format, command.Name, command.Type);
        }

        static IEnumerable<ICommandInputDescriptor> NonPositionalParameters(ICommandDescriptor command)
        {
            return command.Inputs
                .Select(x => x.Value)
                .Where(x => x.Position == null && x.IsRequired)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Concat(
                    command.Inputs.Select(x => x.Value)
                        .Where(x => x.Position == null && x.IsRequired == false)
                        .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                );
        }

        static IEnumerable<ICommandInputDescriptor> PositionalParameters(ICommandDescriptor command)
        {
            return command.Inputs.Select(x => x.Value)
                .Where(x => x.Position != null)
                .OrderBy(x => x.Position);
        }

        string CreateParameters(ICommandDescriptor command)
        {
            return PositionalParameters(command)
                .Concat(NonPositionalParameters(command))
                .Select(CreateInputDescription)
                .JoinString("\r\n\r\n");
        }

        string CreatePositionedParameters(ICommandDescriptor command)
        {
            return PositionalParameters(command)
                .Select(CreatePositionedParameter)
                .JoinString(" ");
        }

        string CreateUnpositionedParameters(ICommandDescriptor command)
        {
            return NonPositionalParameters(command)
                .Select(CreateUnpositionedParameter)
                .JoinString(" ");
        }

        string CreateUsageLine(ICommandDescriptor command)
        {
            var positionedParameters = CreatePositionedParameters(command);
            var unpositionedParameters = CreateUnpositionedParameters(command);
            return new[] { CommandName, positionedParameters, unpositionedParameters }.NotNullOrEmpty().JoinString(" ");
        }
    }
}