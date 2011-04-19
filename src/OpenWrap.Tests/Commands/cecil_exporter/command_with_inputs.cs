using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;

namespace Tests.Commands.cecil_exporter
{
    public class command_with_inputs : contexts.cecil_command_exporter
    {
        public command_with_inputs()
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
                        })
                        .Property(
                            towards=>towards
                                .OfType<string>()
                                .Attribute(()=> new CommandInputAttribute
                            {
                                    IsRequired = true,
                                    IsValueRequired = false,
                                    Position = 1
                            })
                        )
                        ));
            when_exporting();
        }

        [Test]
        public void command_input_is_present()
        {
            command("be", "evil").Descriptor.Inputs.ShouldHaveCountOf(1);
        }

        [Test]
        public void command_input_has_correct_attributes()
        {
            command("be", "evil").Descriptor.Inputs.First()
                    .Check(_ => _.Key.ShouldBe("towards"))
                    .Check(_ => _.Value.Name.ShouldBe("towards"))
                    .Check(_ => _.Value.IsRequired.ShouldBeTrue())
                    .Check(_ => _.Value.IsValueRequired.ShouldBeFalse())
                    .Check(_ => _.Value.Position.ShouldBe(1));
        }
    }
}