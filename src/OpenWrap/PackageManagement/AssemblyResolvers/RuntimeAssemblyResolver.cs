using System;
using System.Linq;
using System.Reflection;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement.AssemblyResolvers
{
    public class RuntimeAssemblyResolver : IService
    {
        ILookup<string, Exports.IAssembly> _assemblyReferences;

        protected IEnvironment Environment
        {
            get { return Services.ServiceLocator.GetService<IEnvironment>(); }
        }

        protected IPackageResolver PackageResolver
        {
            get { return Services.ServiceLocator.GetService<IPackageResolver>(); }
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
            throw new NotImplementedException();
            //_assemblyReferences = Environment.Descriptor == null || Environment.Descriptor.UseProjectRepository == false
            //                              ? AssemblyReferences.GetAssemblyReferences(Environment.ExecutionEnvironment, Environment.SystemRepository).ToLookup(x => x.AssemblyName.Name)
            //                              : PackageResolver.GetAssemblyReferences(true,
            //                                                                      Environment.Descriptor,
            //                                                                      Environment.ProjectRepository,
            //                                                                      Environment.SystemRepository).ToLookup(x => x.AssemblyName.Name);
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