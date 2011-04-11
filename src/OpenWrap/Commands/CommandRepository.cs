using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands
{
    public class CommandRepository : ICommandRepository
    {
        readonly HashSet<string> _commandVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        readonly ICollection<ICommandDescriptor> _commands;
        readonly HashSet<string> _nouns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public CommandRepository(IEnumerable<ICommandDescriptor> commands)
        {
            _commands = new List<ICommandDescriptor>();
            foreach (var command in commands)
                Add(command);
        }

        public CommandRepository()
        {
            _commands = new List<ICommandDescriptor>();
        }

        public int Count
        {
            get { return _commands.Count; }
        }

        public bool IsReadOnly
        {
            get { return _commands.IsReadOnly; }
        }

        public IEnumerable<string> Nouns
        {
            get { return _nouns; }
        }

        public IEnumerable<string> Verbs
        {
            get { return _commandVerbs; }
        }

        public void Add(ICommandDescriptor commandDescriptor)
        {
            if (!_nouns.Contains(commandDescriptor.Noun))
                _nouns.Add(commandDescriptor.Noun);
            if (!_commandVerbs.Contains(commandDescriptor.Verb))
                _commandVerbs.Add(commandDescriptor.Verb);

            _commands.Add(commandDescriptor);
        }

        public void Clear()
        {
            _commands.Clear();
        }

        public bool Contains(ICommandDescriptor item)
        {
            return _commands.Contains(item);
        }

        public void CopyTo(ICommandDescriptor[] array, int arrayIndex)
        {
            _commands.CopyTo(array, arrayIndex);
        }

        public bool Remove(ICommandDescriptor item)
        {
            return _commands.Remove(item);
        }

        public ICommandDescriptor Get(string verb, string name)
        {
            return _commands.FirstOrDefault(x => x.Noun.EqualsNoCase(verb)
                                                 && x.Verb.EqualsNoCase(name));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ICommandDescriptor>)this).GetEnumerator();
        }

        IEnumerator<ICommandDescriptor> IEnumerable<ICommandDescriptor>.GetEnumerator()
        {
            return _commands.GetEnumerator();
        }

        public void Initialize()
        {
        }
    }
}