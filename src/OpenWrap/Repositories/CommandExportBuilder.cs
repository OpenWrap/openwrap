using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
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
}