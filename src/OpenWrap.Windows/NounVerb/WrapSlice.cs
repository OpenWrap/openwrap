using System.Collections.Generic;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.Runtime;

namespace OpenWrap.Windows.NounVerb
{
    public class WrapSlice : NounSlice
    {
        private DependencyResolutionResult _projectDependencies;

        public WrapSlice(string noun, IEnumerable<VerbSlice> commandDescriptors)
            : base(noun, commandDescriptors)
        {

            if (Environment != null && Environment.ProjectRepository != null)
            {
                ProjectDependencies = PackageResolver.TryResolveDependencies(Environment.Descriptor, new[] { Environment.ProjectRepository });
            }
        }

        public DependencyResolutionResult ProjectDependencies
        {
            get
            {
                return _projectDependencies;
            }
            set
            {
                _projectDependencies = value; 
                RaisePropertyChanged("ProjectDependencies");
            }
        }

        protected IEnvironment Environment { get { return Services.ServiceLocator.GetService<IEnvironment>(); } }
        protected IPackageResolver PackageResolver { get { return Services.ServiceLocator.GetService<IPackageResolver>(); } }
    }
}