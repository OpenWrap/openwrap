using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.Commands;
using Tests.Commands.usage;

namespace getHelpCommand_specs
{
    public class when_parameter_required : contexts.help
    {
        public when_parameter_required()
        {
            given_command("ring", "put");
            given_parameter("finger", typeof(string), isRequired: true, isValueRequired: true);
            when_getting_help();
        }

        [Test]
        public void parameter_is_shown_as_required_with_required_value()
        {
            Usage.ShouldBe("put-ring -finger <String>");

        }
    }
    public class when_parameter_optional_with_required_value : contexts.help
    {
        public when_parameter_optional_with_required_value()
        {
            given_command("ring", "put");
            given_parameter("finger", typeof(string), isRequired: false, isValueRequired: true);
            when_getting_help();

        }

        [Test]
        public void parameter_is_shown_as_optional_with_required_value()
        {
            Usage.ShouldBe("put-ring [-finger <String>]");
        }
    }

    internal class when_parameter_required_with_optional_value : contexts.help
    {
        public when_parameter_required_with_optional_value()
        {
            given_command("ring", "put");
            given_parameter("finger", typeof(string), isRequired: true, isValueRequired: false);
            when_getting_help();

        }

        [Test]
        public void parameter_is_shown_in_bracket_with_type_without_brackets()
        {
            Usage.ShouldBe("put-ring -finger [<String>]");
        }
    }

    public class when_required_positioned_parameters : contexts.help
    {
        public when_required_positioned_parameters()
        {
            given_command("ring", "put");
            given_parameter("finger", typeof(string), isRequired: true, isValueRequired: true, position: 0);
            given_parameter("metal", typeof(string), isRequired: true, isValueRequired: true, position: 1);
            when_getting_help();

        }

        [Test]
        public void parameters_are_in_order()
        {
            Usage.ShouldBe("put-ring [-finger] <String> [-metal] <String>");

        }
    }
    public class when_optional_positioned_parameters : contexts.help
    {
        public when_optional_positioned_parameters()
        {
            given_command("ring", "put");
            given_parameter("finger", typeof(string), isRequired: false, isValueRequired: true, position: 0);
            given_parameter("metal", typeof(string), isRequired: false, isValueRequired: true, position: 1);
            when_getting_help();

        }

        [Test]
        public void parameters_are_in_order()
        {
            Usage.ShouldBe("put-ring [[-finger] <String>] [[-metal] <String>]");

        }
    }
    public class when_optional_positioned_parameters_with_optional_values : contexts.help
    {
       
        public when_optional_positioned_parameters_with_optional_values()
        {
            given_command("ring", "put");
            given_parameter("finger", typeof(string), isRequired: false, isValueRequired: false, position: 0);
            when_getting_help();

        }

        [Test]
        public void parameter_is_either_named_or_not_named()
        {
            Usage.ShouldBe("put-ring (-finger [<String>] | <String>)");
        }
    }
    public class when_required_and_positioned_parameters : contexts.help
    {
        public when_required_and_positioned_parameters()
        {
            given_command("ring", "put");
            given_parameter("recipient", typeof(string), isRequired: true, isValueRequired: true);
            given_parameter("finger", typeof(string), isRequired: true, isValueRequired: true, position: 0);
            given_parameter("metal", typeof(string), isRequired: false, isValueRequired: true, position: 1);
            when_getting_help();
        }

        [Test]
        public void positional_parameters_are_shown_first()
        {
            Usage.ShouldBe("put-ring [-finger] <String> [[-metal] <String>] -recipient <String>");
        }
    }
    public class when_optional_and_positioned_parameters : contexts.help
    {
        public when_optional_and_positioned_parameters()
        {
            given_command("ring", "put");
            given_parameter("recipient", typeof(string), isRequired: false, isValueRequired: true);
            given_parameter("finger", typeof(string), isRequired: true, isValueRequired: true, position: 0);
            when_getting_help();
        }

        [Test]
        public void positional_parameters_are_shown_first()
        {
            Usage.ShouldBe("put-ring [-finger] <String> [-recipient <String>]");
        }
    }
    namespace contexts
    {
        public abstract class help
        {
            List<ICommandInputDescriptor> _inputs = new List<ICommandInputDescriptor>();
            protected string Usage;
            MemoryCommandDescriptor _command;

            public void given_command(string noun, string verb)
            {
                _command = new MemoryCommandDescriptor() { Noun = noun, Verb = verb };
            }
            public void given_parameter(string name, Type type, bool isRequired, bool isValueRequired, string description = null, int? position = null)
            {
                _command.AllInputs.Add(new MemoryCommandInput
                {
                        Name = name,
                        Type = type.Name,
                        IsRequired = isRequired,
                        IsValueRequired = isValueRequired,
                        Description = description ?? string.Empty,
                        Position = position
                });
            }

            protected void when_getting_help()
            {
                Usage = new CommandDescriptionOutput(_command).UsageLine;
            }
        }
    }
}