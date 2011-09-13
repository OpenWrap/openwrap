using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement.AssemblyResolvers
{
    public class RuntimeAssemblyResolver : IService
    {
        ILookup<string, IAssemblyReferenceExportItem> _assemblyReferences;

        protected IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }
        }

        protected IPackageResolver PackageResolver
        {
            get { return Services.Services.GetService<IPackageResolver>(); }
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

            _assemblyReferences = Environment.Descriptor == null || Environment.Descriptor.UseProjectRepository == false
                                          ? AssemblyReferences.GetAssemblyReferences(Environment.ExecutionEnvironment, Environment.SystemRepository).ToLookup(x => x.AssemblyName.Name)
                                          : PackageResolver.GetAssemblyReferences(true,
                                                                                  Environment.ExecutionEnvironment,
                                                                                  Environment.Descriptor,
                                                                                  Environment.ProjectRepository,
                                                                                  Environment.SystemRepository)
                                                                                  .ToLookup(x => x.AssemblyName.Name);
        }

        Assembly TryResolveAssembly(object sender, ResolveEventArgs args)
        {
            var simpleName = new AssemblyName(args.Name).Name;
            var existingAssembly =
                    AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals(simpleName, StringComparison.OrdinalIgnoreCase));
            if (existingAssembly != null) return existingAssembly;
            EnsureAssemblyReferencesAreLoaded();
            if (_assemblyReferences.Contains(simpleName))
                return Assembly.LoadFrom(_assemblyReferences[simpleName].First().FullPath);

            return null;
        }

        Assembly TryResolveReflectionOnlyAssembly(object sender, ResolveEventArgs args)
        {
            EnsureAssemblyReferencesAreLoaded();
            // get the simple name of the assembly
            var simpleName = new AssemblyName(args.Name).Name;
            if (_assemblyReferences.Contains(simpleName))
                return Assembly.ReflectionOnlyLoadFrom(_assemblyReferences[simpleName].First().FullPath);

            return null;
        }
    }
}