using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;

namespace command_description_spec
{
    public class command_without_options : context.command_description
    {
        public command_without_options()
        {
            given_command("help", "get");
            when_getting_help();
        }
        [Test]
        public void usage_is_correct()
        {
            Help.UsageLine.ShouldBe("get-help");
        }
    }
    public class command_with_positioned_parameters : context.command_description
    {
        public command_with_positioned_parameters()
        {
            given_command("help", "get");
            given_command_input("first", typeof(string), false, 0);
            given_command_input("second", typeof(int), false, 1);
            when_getting_help();
        }
        [Test]
        public void usage_line_is_correct()
        {
            Help.UsageLine.ShouldBe("get-help [[-first] <String>] [[-second] <Int32>]");
        }
    }

    public class command_with_positioned_and_other_parameters : context.command_description
    {
        public command_with_positioned_and_other_parameters()
        {
            given_command("help", "get");
            given_command_input("positional", typeof(string), true, 0);
            given_command_input("required", typeof(string), true, null);
            given_command_input("required2", typeof(string), true, null);
            given_command_input("optional", typeof(string), false, null);
            when_getting_help();
        }
        [Test]
        public void positionals_then_required_then_optional()
        {
            Help.UsageLine.ShouldBe("get-help [-positional] <String> -required <String> -required2 <String> [-optional <String>]");
        }
    }
    public class command_with_positioned_and_optional_parameters : context.command_description
    {
        public command_with_positioned_and_optional_parameters()
        {
            given_command("help", "get");
            given_command_input("first", typeof(string), true, 0);
            given_command_input("second", typeof(int), false, 1);
            when_getting_help();
        }
        [Test]
        public void usage_line_is_correct()
        {
            Help.UsageLine.ShouldBe("get-help [-first] <String> [[-second] <Int32>]");
        }
    }
    namespace context
    {
        public abstract class command_description : OpenWrap.Testing.context
        {
            protected void when_getting_help()
            {
                Help = new CommandDescriptionOutput(Command);

            }

            protected CommandDescriptionOutput Help { get; set; }
            protected MemoryCommandDescriptor Command { get; set; }

            protected void given_command(string noun, string verb)
            {
                Command = new MemoryCommandDescriptor { Noun = noun, Verb = verb };
            }

            protected void given_command_input(string name, Type type, bool required, int? position)
            {
                Command.AllInputs.Add(new MemoryCommandInput { Name = name, Type = type, Position = position, IsRequired=required });
            }
        }
    }
    public class MemoryCommandDescriptor : ICommandDescriptor
    {
        public string Noun { get; set; }
        public string Verb { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public IDictionary<string, ICommandInputDescriptor> Inputs { get { return AllInputs.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase); } }
        public IList<ICommandInputDescriptor> AllInputs { get; private set; }

        public MemoryCommandDescriptor()
        {
            AllInputs = new List<ICommandInputDescriptor>();
        }

        public MemoryCommandDescriptor(IEnumerable<ICommandInputDescriptor> inputs)
        {
            AllInputs = new List<ICommandInputDescriptor>(inputs);

        }
        public ICommand Create()
        {
            return null;
        }
    }
    public class MemoryCommandInput : ICommandInputDescriptor
    {
        public bool IsRequired { get; set; }

        public bool IsValueRequired { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? Position { get; set; }
        public Type Type { get; set; }
        public object ValidateValue<T>(T value)
        {
            return null;
        }

        public void SetValue(object target, object value)
        {
        }
    }
}
