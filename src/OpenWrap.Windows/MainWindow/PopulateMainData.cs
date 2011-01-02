using System;
using System.Collections.Generic;
using System.Linq;

using OpenWrap.Collections;
using OpenWrap.Commands;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.NounVerb;
using OpenWrap.Windows.Package;
using OpenWrap.Windows.PackageRepository;

namespace OpenWrap.Windows.MainWindow
{
    public class PopulateMainData : CommandBase<MainViewModel>
    {
        protected override void Execute(MainViewModel parameter)
        {
            // todo: constructor-inject some services
            // also, this could be more than once command since it sets up various data
            var commands = Services.Services.GetService<ICommandRepository>();
            var nouns = commands != null ? RealCommands(commands) : MockCommands();

            parameter.Nouns.Clear();
            parameter.Nouns.AddRange(nouns);

            IEnvironment environment = GetEnvironment();

            if (environment != null)
            {
                parameter.PackageRepositories.Clear();
                parameter.PackageRepositories.AddRange(ReadPackageRepositories(environment.RemoteRepositories));

                parameter.SystemPackages.Clear();
                parameter.SystemPackages.AddRange(ReadPackages(environment.SystemRepository));

                parameter.ProjectPackages.Clear();
                parameter.ProjectPackages.AddRange(ReadPackages(environment.ProjectRepository));
            }
        }

        private static IEnvironment GetEnvironment()
        {
            var environment = new CurrentDirectoryEnvironment();
            environment.Initialize();
            return environment;
        }

        private static NounSlice CreateNounSlice(IGrouping<string, ICommandDescriptor> x)
        {
            if (x.Key.Equals("wrap", StringComparison.OrdinalIgnoreCase))
                return new WrapSlice(x.Key, x.Select(y => new VerbSlice(y)));
            return new NounSlice(x.Key, x.Select(y => new VerbSlice(y)));
        }

        private static IEnumerable<NounSlice> RealCommands(IEnumerable<ICommandDescriptor> commands)
        {
            return commands.GroupBy(x => x.Noun).Select(CreateNounSlice);
        }

        private static IEnumerable<NounSlice> MockCommands()
        {
            yield return new NounSlice("Test 1", new[] { new VerbSlice(new NullCommandDescriptor()) });
            yield return new NounSlice("Test 2", new[] { new VerbSlice(new NullCommandDescriptor()) });
        }

        private static IEnumerable<PackageViewModel> TranslatePackages(IEnumerable<IGrouping<string, IPackageInfo>> packageGroups)
        {
            List<PackageViewModel> result = new List<PackageViewModel>();

            foreach (IGrouping<string, IPackageInfo> packageGroup in packageGroups)
            {
                foreach (var packageInfo in packageGroup)
                {
                    PackageViewModel viewModel = TranslatePackage(packageInfo);
                    result.Add(viewModel);
                }
            }

            return result;
        }

        private static IEnumerable<PackageGroupViewModel> TranslateAndGroupPackages(IEnumerable<IGrouping<string, IPackageInfo>> packageGroups)
        {
            List<PackageGroupViewModel> result = new List<PackageGroupViewModel>();

            foreach (IGrouping<string, IPackageInfo> packageGroup in packageGroups)
            {
                PackageGroupViewModel packageGroupViewModel = new PackageGroupViewModel();
                packageGroupViewModel.Name = packageGroup.Key;
                foreach (var packageInfo in packageGroup)
                {
                    PackageViewModel viewModel = TranslatePackage(packageInfo);
                    packageGroupViewModel.Versions.Add(viewModel);
                }

                result.Add(packageGroupViewModel);
            }

            return result;
        }

        private static PackageViewModel TranslatePackage(IPackageInfo packageInfo)
        {
            return new PackageViewModel
            {
                Name = packageInfo.Name,
                FullName = packageInfo.FullName,
                Description = packageInfo.Description,
                ShortVersion = packageInfo.Version.ToString(),
                Version = "Version " + packageInfo.Version,
                Created = packageInfo.Created,
                Anchored = packageInfo.Anchored,
                Nuked = packageInfo.Nuked
            };
        }

        private static IEnumerable<PackageViewModel> ReadPackages(IPackageRepository repository)
        {
            return TranslatePackages(repository.PackagesByName.NotNull());
        }

        private static IEnumerable<PackageRepositoryViewModel> ReadPackageRepositories(IEnumerable<IPackageRepository> remoteRepositories)
        {
            List<PackageRepositoryViewModel> results = new List<PackageRepositoryViewModel>();

            foreach (var packageRepository in remoteRepositories)
            {
                PackageRepositoryViewModel viewModel = new PackageRepositoryViewModel
                {
                    Name = packageRepository.Name
                };

                IEnumerable<PackageGroupViewModel> packages = TranslateAndGroupPackages(packageRepository.PackagesByName.NotNull());
                viewModel.PackageGroups.AddRange(packages);
                viewModel.UpdatePackagesCountText();

                results.Add(viewModel);
            }

            return results;
        }
    }
}
