using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public static class ExportBuilders
    {
        static readonly List<IExportBuilder> _exportBuilders = new List<IExportBuilder>
        {
            new AssemblyReferenceExportBuilder(),
            new CommandExportBuilder()
        };
        public static ICollection<IExportBuilder> All { get { return _exportBuilders; } }
    }

    internal class CommandExportBuilder
        : IExportBuilder
    {
        public string ExportName
        {
            get { return "commands"; }
        }

        public bool CanProcessExport(string exportName)
        {
            return exportName.Equals(exportName, StringComparison.OrdinalIgnoreCase);
        }

        public IExport ProcessExports(IEnumerable<IExport> exports, ExecutionEnvironment environment)
        {
            var commandTypes = from folder in exports
                             from file in folder.Items
                             where file.FullPath.EndsWith(".dll")
                             let assembly = TryReflectionOnlyLoad(file)
                             where assembly != null
                             from type in assembly.GetExportedTypes()
                             where type.GetInterface("OpenWrap.Commands.ICommand, OpenWrap") != null
                             let loadedAssembly = Assembly.LoadFrom(file.FullPath)
                             select loadedAssembly.GetType(type.FullName);
            return new CommandExport(commandTypes);
        }

        Assembly TryReflectionOnlyLoad(IExportItem file)
        {
            try
            {
                return Assembly.ReflectionOnlyLoadFrom(file.FullPath);
            }
            catch{
                return null;
            }
        }
    }

    internal class CommandExport : IExport
    {
        public CommandExport(IEnumerable<Type> commandTypes)
        {
            Items = commandTypes.Select(x => (IExportItem)new CommandExportItem(x)).ToList();
        }

        public string Name
        {
            get { return "Commands"; }
        }

        public IEnumerable<IExportItem> Items
        {
            get; private set;
        }
    }

    public interface ICommandExportItem
    {
        ICommandDescriptor Descriptor { get; }
    }

    public class CommandExportItem : IExportItem, ICommandExportItem
    {
        public ICommandDescriptor Descriptor { get; private set; }
        public CommandExportItem(Type commandTypes)
        {
            Descriptor = new AttributeBasedCommandDescriptor(commandTypes);
            FullPath = commandTypes.Assembly.Location;
        }

        public string FullPath
        {
            get; private set;
        }
    }
}