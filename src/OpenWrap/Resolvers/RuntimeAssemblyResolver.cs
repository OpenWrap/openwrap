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
        protected IPackageManager PackageManager { get { return WrapServices.GetService<IPackageManager>(); } }
        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        public void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += TryResolveAssembly;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += TryResolveReflectionOnlyAssembly;
            _assemblyReferences = PackageManager.GetAssemblyReferences(Environment.ExecutionEnvironment, Environment.ProjectRepository, Environment.SystemRepository).ToLookup(x=>x.AssemblyName.Name);
        }

        Assembly TryResolveReflectionOnlyAssembly(object sender, ResolveEventArgs args)
        {
            // get the simple name of the assembly
            var simpleName = new AssemblyName(args.Name).Name;
            if (_assemblyReferences.Contains(simpleName))
                return Assembly.ReflectionOnlyLoadFrom(_assemblyReferences[simpleName].First().FullPath);
            
            return null;
        }

        Assembly TryResolveAssembly(object sender, ResolveEventArgs args)
        {
            var simpleName = new AssemblyName(args.Name).Name;
            if (_assemblyReferences.Contains(simpleName))
                return Assembly.LoadFrom(_assemblyReferences[simpleName].First().FullPath);
            
            return null;
        }
    }
}
