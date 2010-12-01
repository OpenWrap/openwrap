using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Windows.Framework;

namespace OpenWrap.Windows
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PackageViewModel> _packages = new ObservableCollection<PackageViewModel>();
        private IEnumerable<NounSlice> _nouns;
        private NounSlice _selectedNoun;

        public MainViewModel()
        {
            var commands = Services.Services.GetService<ICommandRepository>();
            Nouns = commands != null ? RealCommands(commands) : MockCommands();

            var env = Services.Services.GetService<IEnvironment>();

            ReadPackages(env.ProjectRepository.FindAll(new PackageDependency()));
        }

        private void ReadPackages(IEnumerable<IPackageInfo> packages)
        {
            foreach (var packageInfo in packages)
            {
                PackageViewModel viewModel = new PackageViewModel
                {
                        Name = packageInfo.Name,
                        FullName = packageInfo.FullName,
                        Description = packageInfo.Description,
                        Version = packageInfo.Version,
                        CreationTime = packageInfo.CreationTime,
                        Anchored = packageInfo.Anchored,
                        Nuked = packageInfo.Nuked
                };

                _packages.Add(viewModel);
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

        public ObservableCollection<PackageViewModel> Packages
        {
            get { return _packages; }
        }

        private static NounSlice CreateNounSlice(IGrouping<string, ICommandDescriptor> x)
        {
            if (x.Key.Equals("wrap", StringComparison.OrdinalIgnoreCase))
                return new WrapSlice(x.Key, x.Select(y => new VerbSlice(y)));
            return new NounSlice(x.Key, x.Select(y => new VerbSlice(y)));
        }

        private static IEnumerable<NounSlice> RealCommands(ICommandRepository commands)
        {
            return commands.GroupBy(x => x.Noun).Select(x => CreateNounSlice(x));
        }
        
        private static IEnumerable<NounSlice> MockCommands()
        {
            yield return new NounSlice("Test 1", new[] { new VerbSlice(new InMemoryCommandDescriptor()) });
            yield return new NounSlice("Test 2", new[] { new VerbSlice(new InMemoryCommandDescriptor()) });
        }
    }
}
