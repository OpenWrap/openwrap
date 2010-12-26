using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Repositories;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Package;
using OpenWrap.Windows.PackageRepository;

namespace OpenWrap.Windows
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageRepositoryViewModel> _packageRepositories = new ObservableCollection<PackageRepositoryViewModel>();
        private readonly ObservableCollection<PackageViewModel> _systemPackages = new ObservableCollection<PackageViewModel>();
        private readonly ObservableCollection<PackageViewModel> _projectPackages = new ObservableCollection<PackageViewModel>();

        private readonly AddPackageRepositoryDialogCommand _addPackageRepositoryDialogCommand = new AddPackageRepositoryDialogCommand();

        private IEnumerable<NounSlice> _nouns;
        private NounSlice _selectedNoun;

        public MainViewModel()
        {
            var commands = Services.Services.GetService<ICommandRepository>();
            Nouns = commands != null ? RealCommands(commands) : MockCommands();

            var env = Services.Services.GetService<IEnvironment>();

            if (env != null)
            {
                ReadPackageRepositories(env.RemoteRepositories);
                ReadPackages(env.SystemRepository, _systemPackages);
                ReadPackages(env.ProjectRepository, _projectPackages);
            }
        }

        public IEnumerable<NounSlice> Nouns
        {
            get
            {
                return _nouns;
            }
            set
            {
                _nouns = value;
                RaisePropertyChanged<MainViewModel>(o => o.Nouns);
            }
        }

        public NounSlice SelectedNoun
        {
            get
            {
                return _selectedNoun;
            }
            set
            {
                _selectedNoun = value;
                RaisePropertyChanged<MainViewModel>(o => o.SelectedNoun);
            }
        }

        public AddPackageRepositoryDialogCommand AddPackageRepositoryDialogCommand
        {
            get { return _addPackageRepositoryDialogCommand; }
        }

        public ObservableCollection<PackageViewModel> SystemPackages
        {
            get { return _systemPackages; }
        }

        public ObservableCollection<PackageViewModel> ProjectPackages
        {
            get { return _projectPackages; }
        }

        public ObservableCollection<PackageRepositoryViewModel> PackageRepositories
        {
            get { return _packageRepositories; }
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
            yield return new NounSlice("Test 1", new[] { new VerbSlice(new InMemoryCommandDescriptor()) });
            yield return new NounSlice("Test 2", new[] { new VerbSlice(new InMemoryCommandDescriptor()) });
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

        static PackageViewModel TranslatePackage(IPackageInfo packageInfo)
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

        private static void ReadPackages(IPackageRepository repository, ObservableCollection<PackageViewModel> viewModels)
        {
            var packages = TranslatePackages(repository.PackagesByName.NotNull());
            viewModels.AddRange(packages);
        }
        
        private void ReadPackageRepositories(IEnumerable<IPackageRepository> remoteRepositories)
        {
            foreach (var packageRepository in remoteRepositories)
            {
                PackageRepositoryViewModel viewModel = new PackageRepositoryViewModel
                {
                        Name = packageRepository.Name
                };

                IEnumerable<PackageGroupViewModel> packages = TranslateAndGroupPackages(packageRepository.PackagesByName.NotNull());
                viewModel.PackageGroups.AddRange(packages);

                _packageRepositories.Add(viewModel);
            }
        }
    }
}
