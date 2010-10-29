using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Resolvers
{
    public class RuntimeAssemblyResolver : IService
    {
        ILookup<string, IAssemblyReferenceExportItem> _assemblyReferences;
        protected IPackageManager PackageManager { get { return Services.Services.GetService<IPackageManager>(); } }
        protected IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); } }
        public void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += TryResolveAssembly;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += TryResolveReflectionOnlyAssembly;
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

        Assembly TryResolveAssembly(object sender, ResolveEventArgs args)
        {
            EnsureAssemblyReferencesAreLoaded();
            var simpleName = new AssemblyName(args.Name).Name;
            if (_assemblyReferences.Contains(simpleName))
                return Assembly.LoadFrom(_assemblyReferences[simpleName].First().FullPath);
            
            return null;
        }
        void EnsureAssemblyReferencesAreLoaded()
        {
            if (_assemblyReferences != null)
                return;
            if (Environment.Descriptor == null)
                _assemblyReferences = 
            _assemblyReferences = Environment.Descriptor == null
                ? PackageManager.GetAssemblyReferences(Environment.ExecutionEnvironment, Environment.SystemRepository).ToLookup(x => x.AssemblyName.Name)
                : PackageManager.GetAssemblyReferences(true, Environment.ExecutionEnvironment, Environment.Descriptor, Environment.ProjectRepository, Environment.SystemRepository).ToLookup(x => x.AssemblyName.Name);
        }
    }
}
