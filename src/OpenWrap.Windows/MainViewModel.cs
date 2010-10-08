using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Windows
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            var commands = WrapServices.GetService<ICommandRepository>();
            Nouns = commands != null 
                ? commands.GroupBy(x => x.Noun).Select(x => CreateNounSlice(x)).ToList()
                : MockCommands();
        }

        NounSlice CreateNounSlice(IGrouping<string, ICommandDescriptor> x)
        {
            if (x.Key.Equals("wrap", StringComparison.OrdinalIgnoreCase))
                return new WrapSlice(x.Key, x.Select(y => new VerbSlice(y)));
            return new NounSlice(x.Key,x.Select(y=>new VerbSlice(y)));
        }

        IEnumerable<NounSlice> MockCommands()
        {
            yield return new NounSlice("Test 1", new[] { new VerbSlice(new InMemoryCommandDescriptor()) });
            yield return new NounSlice("Test 2", new[] { new VerbSlice(new InMemoryCommandDescriptor()) });
        }

        IEnumerable<NounSlice> _nouns;
        public IEnumerable<NounSlice> Nouns
        {
            get { return _nouns; }
            set { _nouns = value; PropertyChanged(this, new PropertyChangedEventArgs("Nouns")); }
        }

        NounSlice _selectedNoun;
        public NounSlice SelectedNoun
        {
            get { return _selectedNoun; }
            set { _selectedNoun = value; PropertyChanged(this, new PropertyChangedEventArgs("SelectedNoun")); }
        }

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
    }

    internal class InMemoryCommandDescriptor : ICommandDescriptor
    {
        public string Noun { get; private set; }
        public string Verb { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public IDictionary<string, ICommandInputDescriptor> Inputs { get; private set; }
        public ICommand Create()
        {
            return null;
        }
    }

    public class NounSlice : INotifyPropertyChanged
    {
        public NounSlice(string noun, IEnumerable<VerbSlice> commandDescriptors)
        {
            Noun = noun;
            Commands = commandDescriptors;
        }

        public IEnumerable<VerbSlice> Commands { get; set; }

        public string Noun { get; set; }
        public event PropertyChangedEventHandler PropertyChanged = (s,e)=>{};
        protected virtual void NotifyPropertyChanged(string propertyName) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName));}
    }
    
    public class WrapSlice : NounSlice
    {
        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        protected IPackageManager PackageManager { get { return WrapServices.GetService<IPackageManager>(); } }

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
                ProjectDependencies = PackageManager.TryResolveDependencies(Environment.Descriptor, new[] { Environment.ProjectRepository });
            }
        }

    }

    public class LocalDependencyViewModel
    {
    }

    public class VerbSlice
    {
        public VerbSlice(ICommandDescriptor commandDescriptor)
        {
            Verb = commandDescriptor.Verb;
            this.Command = commandDescriptor;
        }

        public ICommandDescriptor Command { get; set; }

        public string Verb { get; set; }
    }
    public class DependenciesViewModel
    {
    }

}
