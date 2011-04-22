using System;
using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.Exporters.Commands
{
    public class CommandDescriptor : ICommandDescriptor
    {
        readonly Func<ICommand> _factory;

        public CommandDescriptor(Func<ICommand> factory, string noun, string verb)
        {
            _factory = factory;
            Noun = noun;
            Verb = verb;
            Inputs = new Dictionary<string, ICommandInputDescriptor>();
        }
        protected CommandDescriptor(Func<ICommand> factory)
        {
            _factory = factory;
        }
        public virtual string Noun { get; protected set; }
        public virtual string Verb { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual IDictionary<string, ICommandInputDescriptor> Inputs { get; private set; }
        public virtual ICommand Create()
        {
            return _factory();
        }
    }
}