using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;
using Tests;

namespace Tests.Commands.exporter
{
    public class command_without_inputs : contexts.cecil_command_exporter
    {
        public command_without_inputs()
        {
            given_package_assembly(
                "commands",
                sauron_commands => sauron_commands(
                    be_evil_command=>be_evil_command
                        .Namespace("Sauron.Commands")
                        .Implements<ICommand>()
                        .Attribute(()=> new CommandAttribute
                        {
                                Noun = "evil",
                                Verb= "be"
                        })));
            when_exporting();
        }

        [Test]
        public void command_is_exported()
        {
            command("be", "evil").ShouldNotBeNull();
        }

        [Test]
        public void command_has_no_input()
        {
            command("be", "evil").Descriptor.Inputs.ShouldBeEmpty();
        }
    }
}