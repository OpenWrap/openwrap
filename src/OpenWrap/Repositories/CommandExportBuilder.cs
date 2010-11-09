using System;
using System.Collections.Generic;
using System.IO;
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
            return ExportName.Equals(exportName, StringComparison.OrdinalIgnoreCase);
        }

        // TODO: Make sure assemblies already loaded are loaded from normal reflection context
        public IExport ProcessExports(IEnumerable<IExport> exports, ExecutionEnvironment environment)
        {
            var loadedAssemblyPaths = AppDomain.CurrentDomain.GetAssemblies().Select(TryGetAssemblyLocation).NotNull().ToList();
            var commandTypes = from folder in exports
                               from file in folder.Items
                               where file.FullPath.EndsWith(".dll")
                               where !loadedAssemblyPaths.Contains(file.FullPath)
                               let assembly = TryReflectionOnlyLoad(file)
                               where assembly != null
                               from type in TryGetExportedTypes(assembly)
                               where type.IsAbstract == false &&
                                     type.IsGenericTypeDefinition == false &&
                                     type.GetInterface("ICommand") != null
                               let loadedAssembly = Assembly.LoadFrom(file.FullPath)
                               select loadedAssembly.GetType(type.FullName);
            return new CommandExport(commandTypes);
        }
        static string TryGetAssemblyLocation(Assembly assembly)
        {
            {
                try
                {
                    return Path.GetFullPath(assembly.Location);
                }
                catch
                {
                    return null;
                }

            }
        }
        static IEnumerable<Type> TryGetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }
        Assembly TryReflectionOnlyLoad(IExportItem file)
        {
            try
            {
                return Assembly.ReflectionOnlyLoadFrom(file.FullPath);
            }
            catch
            {
                return null;
            }
        }
    }
}