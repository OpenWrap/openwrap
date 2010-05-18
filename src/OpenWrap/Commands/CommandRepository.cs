using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public class CommandRepository : ICommandRepository
    {
        readonly ICollection<ICommandDescriptor> _commands = new List<ICommandDescriptor>();
        readonly HashSet<string> _commandVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        readonly HashSet<string> _namespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> Namespaces
        {
            get { return _namespaces; }
        }

        public IEnumerable<string> Verbs
        {
            get { return _commandVerbs; }
        }

        public void Add(ICommandDescriptor commandDescriptor)
        {
            if (!_namespaces.Contains(commandDescriptor.Namespace))
                _namespaces.Add(commandDescriptor.Namespace);
            if (!_commandVerbs.Contains(commandDescriptor.Verb))
                _commandVerbs.Add(commandDescriptor.Verb);

            _commands.Add(commandDescriptor);
        }

        public ICommandDescriptor Get(string @namespace, string name)
        {
            return _commands.Single(x => string.Compare(x.Namespace, @namespace, StringComparison.OrdinalIgnoreCase) == 0
                                         && string.Compare(x.Verb, name, StringComparison.OrdinalIgnoreCase) == 0);
        }

        IEnumerator<ICommandDescriptor> IEnumerable<ICommandDescriptor>.GetEnumerator()
        {
            return _commands.GetEnumerator();
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

        public int Count
        {
            get { return _commands.Count; }
        }

        public bool IsReadOnly
        {
            get { return _commands.IsReadOnly; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ICommandDescriptor>)this).GetEnumerator();
        }

        public void Initialize()
        {
        }
    }
}