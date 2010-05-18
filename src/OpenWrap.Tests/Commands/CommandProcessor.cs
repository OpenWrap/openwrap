using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Repositories.Testing;
using OpenWrap.Commands;

namespace OpenWrap.Repositories.Wrap.Tests.Commands
{
    public class when_finding_commands : context.command_processor
    {
        [Test]
        public void the_command_is_found_by_name_and_namespace()
        {
            given_command<TravelToMordor>();

            when_parsing_input("lotr", "traveltomordor");

            result.ShouldBeOfType<Success>();
        }
        [Test]
        public void an_unknown_namespace_is_not_found()
        {
            given_command<TravelToMordor>();

            when_parsing_input("semarillon", "traveltomordor");

            result.ShouldBeOfType<NamesapceNotFound>();
        }
        [Test]
        public void namespace_entered_partially_is_found()
        {
            given_command<TravelToMordor>();

            when_parsing_input("lo", "traveltomordor");

            result.ShouldBeOfType<Success>();
        }
        [Test]
        public void command_entered_partially_is_found()
        {
            given_command<TravelToMordor>();

            when_parsing_input("l", "t");

            result.ShouldBeOfType<Success>();
        }
        [Test]
        public void unknown_command_is_not_found()
        {
            given_command<TravelToMordor>();

            when_parsing_input("lotr", "traveltooxford");

            result.ShouldBeOfType<UnknownCommand>();
        }

    }
    public class when_assigning_parameters : context.command_processor
    {
        [Test]
        public void parameter_is_assigned_by_name()
        {
            given_command<TravelToMordor>();

            when_parsing_input("lotr", "travel", "-hasring", "true");

            result.ShouldBeOfType<Success>()
                .Command.ShouldBeOfType<TravelToMordor>()
                    .HasRing.ShouldBeTrue();
        }
        [Test]
        public void one_parameter_is_assigned_by_order()
        {

            given_command<TravelToMordor>();

            when_parsing_input("lotr", "travel", "true");

            result.ShouldBeOfType<Success>()
                .Command.ShouldBeOfType<TravelToMordor>()
                    .HasRing.ShouldBeTrue();
        }
        [Test]
        public void multiple_parameters_are_assigned_by_order()
        {

            given_command<TravelToMordor>();

            when_parsing_input("lotr", "travel", "false", "true");

            var command = result.ShouldBeOfType<Success>()
                .Command.ShouldBeOfType<TravelToMordor>();
            command.HasRing.ShouldBeFalse();
            command.IsDangerous.ShouldBeTrue();
        }
    }

    [Command(Namespace="lotr")]
    public class TravelToMordor : ICommand
    {
        [CommandInput(Position = 0)]
        public bool HasRing { get; set; }

        [CommandInput(Position = 1)]
        public bool IsDangerous { get; set; }

        public ICommandResult Execute()
        {
            return new Success() { Command = this };
        }
    }

    namespace context
    {
        public abstract class command_processor : Testing.context
        {
            protected CommandRepository commands;
            protected CommandLineProcessor processor;

            public command_processor()
            {
            }
            [SetUp]
            public virtual void given()
            {
                commands = new CommandRepository();
                processor = new CommandLineProcessor(commands);   
            }
            protected void given_command<T>()
            {
                commands.Add(new AttributeBasedCommandDescriptor(typeof(T)));
            }
            protected ICommandResult result;

            protected void when_parsing_input(params string[] command)
            {
                result = processor.Execute(command);
            }
        }
    }
}
