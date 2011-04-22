using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageManagement.Exporters.Commands;
using OpenWrap.Testing;

namespace Tests.Commands
{
    [TestFixture]
    public class command_documentation
    {
        [Datapoints]
        public ICommandDescriptor[] commands =
                GetCommands();

        static ICommandDescriptor[] GetCommands()
        {
            using (var stream = File.OpenRead(typeof(AddWrapCommand).Assembly.Location))
                return CecilCommandExporter.GetCommandsFromAssembly(stream).ToArray();
        }

        [Theory]
        public void command_has_documentation(ICommandDescriptor command)
        {
            command.Description.ShouldNotBeNullOrEmpty(string.Format("command '{0}-{1}' doesn't have a description",command.Verb, command.Noun));
            foreach (var commandInput in command.Inputs)
                commandInput.Value.Description.ShouldNotBeNullOrEmpty(string.Format("command input '{0}' for command '{1}-{2}' doesn't have a description", commandInput.Value.Name,command.Verb, command.Noun));
        }

        [Theory]
        public void ui_command_has_label(ICommandDescriptor command)
        {
            var uiDescriptor = command as IUICommandDescriptor;
            if (uiDescriptor == null) return;

            uiDescriptor.Label.ShouldNotBeNullOrEmpty(string.Format("UI command '{0}-{1}' doesn't have a label", command.Verb, command.Noun));
        }
    }
}
