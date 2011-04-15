using System;
using OpenWrap.Commands.Core;

namespace Tests.Commands.usage.context
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
            Command.AllInputs.Add(new MemoryCommandInput { Name = name, Type = type.FullName, Position = position, IsRequired=required });
        }
    }
}