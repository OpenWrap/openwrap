using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Services;

namespace OpenWrap.Windows
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            var commands = WrapServices.GetService<ICommandRepository>();
            Nouns = commands != null ? commands.GroupBy(x => x.Noun).Select(x => new NounSlice(x.Key,x.Select(y=>new VerbSlice(y)))).ToList()
                : MockCommands();
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
            set { _selectedNoun = value; }
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

    public class NounSlice
    {
        public NounSlice(string noun, IEnumerable<VerbSlice> commandDescriptors)
        {
            Noun = noun;
            Commands = commandDescriptors;
        }

        public IEnumerable<VerbSlice> Commands { get; set; }

        public string Noun { get; set; }
    }
    
    public class WrapSlice : NounSlice
    {
        public WrapSlice(string noun, IEnumerable<VerbSlice> commandDescriptors) : base(noun, commandDescriptors)
        {
        }

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
        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }

        public IEnumerable<PackageDependency> ProjectDependencies { get { return Environment.Descriptor.Dependencies; } }
        public IEnumerable<SystemPackage> SystemDependencies { get { return Environment.SystemRepository.PackagesByName; } }
    }

    public class SystemPackage
    {
    }
}
