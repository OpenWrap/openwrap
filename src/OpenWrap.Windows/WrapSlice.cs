using System.Collections.Generic;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.Windows
{
    public class WrapSlice : NounSlice
    {
        protected IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); } }
        protected IPackageResolver PackageResolver { get { return Services.Services.GetService<IPackageResolver>(); } }

        DependencyResolutionResult _projectDependencies;
        public DependencyResolutionResult ProjectDependencies
        {
            get { return _projectDependencies; }
            set { _projectDependencies = value; NotifyPropertyChanged("ProjectDependencies"); }
        }

        //public IEnumerable<SystemPackage> SystemDependencies { get { return Environment.SystemRepository.PackagesByName.Select(x => new SystemPackage(x.Key, x)); } }
        public WrapSlice(string noun, IEnumerable<VerbSlice> commandDescriptors) : base(noun, commandDescriptors)
        {

            if (Environment != null && Environment.ProjectRepository != null)
            {
                ProjectDependencies = PackageResolver.TryResolveDependencies(Environment.Descriptor, new[] { Environment.ProjectRepository });
            }
        }

    }
}