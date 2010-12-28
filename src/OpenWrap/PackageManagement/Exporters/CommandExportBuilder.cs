using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters
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
            return ExportName.EqualsNoCase(exportName);
        }

        // TODO: Make sure assemblies already loaded are loaded from normal reflection context
        public IExport ProcessExports(IEnumerable<IExport> exports, ExecutionEnvironment environment)
        {
            var loadedAssemblyPaths = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(x => new { loc = TryGetAssemblyLocation(x), asm = x })
                    .Where(x => x.loc != null)
                    .ToLookup(x => x.loc, x => x.asm, StringComparer.OrdinalIgnoreCase);

            var commandTypes = from folder in exports
                               from file in folder.Items
                               where file.FullPath.EndsWith(".dll")
                               let assembly = loadedAssemblyPaths.Contains(file.FullPath)
                                                      ? loadedAssemblyPaths[file.FullPath].First()
                                                      : TryReflectionOnlyLoad(file)
                               where assembly != null
                               from type in TryGetExportedTypes(assembly)
                               where type.IsAbstract == false &&
                                     type.IsGenericTypeDefinition == false &&
                                     TryGet(()=>type.GetInterface("ICommand")) != null
                               let loadedAssembly = TryGet(()=>Assembly.LoadFrom(file.FullPath))
                               where loadedAssembly != null
                               let loadedType = TryGet(()=>loadedAssembly.GetType(type.FullName))
                               where loadedType != null
                               select loadedType;
            return new CommandExport(commandTypes);
        }
        T TryGet<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return default(T);
            }
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