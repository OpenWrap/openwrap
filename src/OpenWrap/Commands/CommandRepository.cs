using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Wrap.Commands;

namespace OpenRasta.Wrap.Console
{
    public class CommandRepository
    {
        readonly List<ICommandDescriptor> _commands = new List<ICommandDescriptor>();
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
            if (!_commandVerbs.Contains(commandDescriptor.Name))
                _commandVerbs.Add(commandDescriptor.Name);

            _commands.Add(commandDescriptor);
        }

        public ICommandDescriptor Get(string @namespace, string name)
        {
            return _commands.Single(x => string.Compare(x.Namespace, @namespace, StringComparison.OrdinalIgnoreCase) == 0
                                         && string.Compare(x.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}