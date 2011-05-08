using System;
using System.Linq;
using System.Reflection;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement.AssemblyResolvers
{
    public class RuntimeAssemblyResolver : IService
    {
        readonly IEnvironment _environment;
        readonly IPackageManager _packageManager;
        ILookup<string, Exports.IAssembly> _assemblyReferences;

        public RuntimeAssemblyResolver()
                : this(ServiceLocator.GetService<IPackageManager>(), ServiceLocator.GetService<IEnvironment>())
        {
        }

        public RuntimeAssemblyResolver(IPackageManager packageManager, IEnvironment environment)
        {
            _packageManager = packageManager;
            _environment = environment;
        }

        public void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += TryResolveAssembly;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += TryResolveReflectionOnlyAssembly;
        }

        void EnsureAssemblyReferencesAreLoaded()
        {
            if (_assemblyReferences != null)
                return;

            _assemblyReferences = (_environment.Descriptor == null || _environment.Descriptor.UseProjectRepository == false
                                           ? _packageManager.GetSystemExports<Exports.IAssembly>(_environment.SystemRepository, _environment.ExecutionEnvironment).SelectMany(_ => _)
                                           : _packageManager.GetProjectAssemblyReferences
                                                     (
                                                             _environment.Descriptor,
                                                             _environment.ProjectRepository,
                                                             _environment.ExecutionEnvironment,
                                                             true
                                                     ))
                    .ToLookup(x => x.AssemblyName.Name);
        }

        System.Reflection.Assembly TryResolveAssembly(object sender, ResolveEventArgs args)
        {
            EnsureAssemblyReferencesAreLoaded();
            var simpleName = new AssemblyName(args.Name).Name;
            if (_assemblyReferences.Contains(simpleName))
                return System.Reflection.Assembly.LoadFrom(_assemblyReferences[simpleName].First().File.Path.FullPath);

            return null;
        }

        System.Reflection.Assembly TryResolveReflectionOnlyAssembly(object sender, ResolveEventArgs args)
        {
            EnsureAssemblyReferencesAreLoaded();
            // get the simple name of the assembly
            var simpleName = new AssemblyName(args.Name).Name;
            if (_assemblyReferences.Contains(simpleName))
                return System.Reflection.Assembly.ReflectionOnlyLoadFrom(_assemblyReferences[simpleName].First().File.Path.FullPath);

            return null;
        }
    }
}