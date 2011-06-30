using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.Runtime;

namespace OpenWrap.Commands.Cli
{
    public class CecilStaticAssemblyResolver : IAssemblyResolver
    {
        readonly IEnumerable<Exports.IAssembly> _allAssemblies;
        readonly IEnvironment _environment;
        readonly IPackageManager _packageManager;

        public CecilStaticAssemblyResolver(IPackageManager packageManager, IEnvironment environment)
        {
            _packageManager = packageManager;
            _environment = environment;
            var allAssemblies = Enumerable.Empty<Exports.IAssembly>();

            if (_environment.ProjectRepository != null)
                allAssemblies = _packageManager.GetProjectAssemblyReferences(_environment.Descriptor, _environment.ProjectRepository, environment.ExecutionEnvironment, true);
            var selectedPackages = allAssemblies.Select(x => x.Package.Name).ToList();
            var systemAssemblies = _packageManager.GetSystemExports<Exports.IAssembly>(_environment.SystemRepository, environment.ExecutionEnvironment)
                .Where(x => selectedPackages.Contains(x.Key) == false)
                .SelectMany(x => x);
            _allAssemblies = allAssemblies.Concat(systemAssemblies).ToList();
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return ReadAssembly(name.Name, ReadingMode.Deferred);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return ReadAssembly(name.Name, parameters.ReadingMode);
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return Resolve(fullName, new ReaderParameters(ReadingMode.Deferred));
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            var assemblyName = fullName.IndexOf(',') != -1 ? fullName.Substring(0, fullName.IndexOf(',')) : fullName;
            return ReadAssembly(assemblyName, parameters.ReadingMode);
        }

        AssemblyDefinition ReadAssembly(string name, ReadingMode readingMode)
        {
            var matchingAssembly = _allAssemblies.FirstOrDefault(x => x.AssemblyName.Name == name);
            return matchingAssembly != null ? AssemblyDefinition.ReadAssembly(matchingAssembly.File.Path.FullPath, new ReaderParameters(readingMode) { AssemblyResolver = this }) : null;
        }
    }
}