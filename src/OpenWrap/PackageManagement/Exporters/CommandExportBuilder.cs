using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenWrap.Collections;
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

            var reflectionOnlyContext = (from folder in exports
                                         from file in folder.Items
                                         where file.FullPath.EndsWith(".dll")
                                         let assembly = loadedAssemblyPaths.Contains(file.FullPath)
                                                               ? loadedAssemblyPaths[file.FullPath].First()
                                                               : TryReflectionOnlyLoad(file)
                                         where assembly != null
                                         select new { file, assembly }).ToList();

            var commandTypes = from commands in
                                   (from asmFile in reflectionOnlyContext.ToList()
                                    let types = from type in TryGetExportedTypes(asmFile.assembly)
                                                where type.IsAbstract == false &&
                                                type.IsGenericTypeDefinition == false &&
                                                TryGet(() => type.GetInterface("ICommand")) != null
                                                select type
                                    where types.Any()
                                    let assembly = TryGet(() => Assembly.LoadFrom(asmFile.file.FullPath))
                                    where assembly != null
                                    select new { assembly, asmFile.file, types = types.NotNull().ToList() }).ToList()
                               from type in commands.types
                               let loadFromContextType = TryGet(() => commands.assembly.GetType(type.FullName))
                               where loadFromContextType != null
                               select loadFromContextType;
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
                var loadedAsm = Assembly.ReflectionOnlyLoadFrom(file.FullPath);
                return loadedAsm;
            }
            catch
            {
                return null;
            }
        }
    }
}