using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.PackageManagement.Exporters.Commands;
using OpenWrap.Testing;

namespace Tests.Commands.contexts
{
    public abstract class cecil_command<T> : context where T:ICommand
    {
        ICommandDescriptor CommandDescriptor;
        protected IEnumerable<ICommandOutput> Results;
        protected T Command;

        public cecil_command()
        {
            CommandDescriptor = CecilCommandExporter.GetCommandFrom<T>();
        }
        protected void when_executing(string line)
        {
            var commandLineRunner = new CommandLineRunner();
            Results = commandLineRunner.Run(CommandDescriptor, line).ToList();
            Command = (T)commandLineRunner.LastCommand;
        }
    }
}