using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "update", Noun = "wrap")]
    public class UpdateWrapCommand : ICommand
    {

        protected IEnvironment Environment
        {
            get { return WrapServices.GetService<IEnvironment>(); }
        }

        protected IPackageManager PackageManager
        {
            get { return WrapServices.GetService<IPackageManager>(); }
        }

        [CommandInput(DisplayName = "System", IsRequired = false, Name="System")]
        public bool System { get; set; }

        public IEnumerable<ICommandResult> Execute()
        {
            if (Environment.ProjectRepository != null)
                return UpdateProjectPackages();
            return UpdateSystemPackages();
        }

        IEnumerable<ICommandResult> UpdateSystemPackages()
        {
            
            WrapDescriptor packagesToSearch = CreateDescriptorForInstalledPackages();
            yield return new Result("Searching for updated packages");


            var resolveResult = PackageManager.TryResolveDependencies(packagesToSearch, Environment.RemoteRepositories);

            foreach (var message in PackageManager.CopyPackagesToRepositories(
                resolveResult, Environment.SystemRepository))
                yield return message;
        }

        WrapDescriptor CreateDescriptorForInstalledPackages()
        {
            var installedPackages = Environment.SystemRepository.PackagesByName.Select(x => x.Key);

            return new WrapDescriptor
            {
                Dependencies = (from package in installedPackages
                                let maxVersion = Environment.SystemRepository.PackagesByName[package]
                                    .OrderByDescending(x => x.Version)
                                    .Select(x => x.Version)
                                    .First()
                                select new WrapDependency
                                {
                                    Name = package,
                                    VersionVertices = { new GreaterThenVersionVertice(maxVersion) }
                                }).ToList()
            };
        }

        IEnumerable<ICommandResult> UpdateProjectPackages()
        {
            var resolvedPackages = PackageManager.TryResolveDependencies(Environment.Descriptor, Environment.RemoteRepositories.Concat(new[] { Environment.SystemRepository }));

            return PackageManager.CopyPackagesToRepositories(
                resolvedPackages,
                Environment.RepositoriesForWrite()
                );
        }
    }
}
