using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.cecil_exporter
{
    public class command_without_inputs : cecil_command_exporter
    {
        public command_without_inputs()
        {
            given_package_assembly(
                    "commands",
                    sauron_commands => sauron_commands(
                            be_evil_command => be_evil_command
                                                       .Implements<ICommand>()
                                                       .Attribute(() => new CommandAttribute
                                                       {
                                                               Noun = "evil",
                                                               Verb = "be"
                                                       })));
            when_exporting();
        }

        [Test]
        public void command_has_no_input()
        {
            command("be", "evil").Descriptor.Inputs.ShouldBeEmpty();
        }

        [Test]
        public void command_is_exported()
        {
            command("be", "evil").ShouldNotBeNull();
        }
    }
}