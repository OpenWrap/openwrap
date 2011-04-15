using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;
using Tests.Commands.runner;
using Tests.Commands.usage;

namespace Tests.Commands.contexts
{
    public abstract class runner : context
    {
        MemoryCommandDescriptor Command;
        protected IEnumerable<ICommandOutput> Results;
        Dictionary<string, List<string>> _properties;
        protected bool CommandExecuted;

        public runner()
        {
            _properties = new Dictionary<string, List<string>>();
            
        }
        protected void when_executing(string line)
        {
            Results = new CommandRunner().Run(Command, line).ToList();
        }
        protected IEnumerable<string> Input(string inputName)
        {
            return _properties.ContainsKey(inputName) ? _properties[inputName] : Enumerable.Empty<string>();
        }
        protected void given_command(string verb, string noun, params Expression<Func<InputBuilder, InputBuilder>>[] properties)
        {
            Command = new MemoryCommandDescriptor(
                    properties.Select(property => property.Compile()(
                            new InputBuilder(property.Parameters[0].Name).Setter((cmd, val) => SaveProperty(property.Parameters[0].Name, val)))
                                                          .Descriptor))
            {
                    Verb = verb,
                    Noun = noun,
                    Create = ()=> new MemoryCommand
                    {
                            Execute = ()=> { 
                                CommandExecuted = true;
                                return Enumerable.Empty<ICommandOutput>();
                            }
                    }
            };
        }

        bool SaveProperty(string name, IEnumerable<string> val)
        {
            _properties.GetOrCreate(name).AddRange(val);
            return true;
        }
        public class InputBuilder
        {
            readonly string _name;
            MemoryCommandInput _descriptor;
            public InputBuilder(string name)
            {
                _descriptor = new MemoryCommandInput { Name = name };
            }
            public InputBuilder Required
            {
                get { _descriptor.IsRequired = true; return this; }
            }
            public InputBuilder OptionalValue
            {
                get { _descriptor.IsValueRequired = false; return this; }
            }
            public InputBuilder Setter(Func<ICommand, IEnumerable<string>, bool> setter)
            {
                _descriptor.TrySetValue = setter;
                return this;
            }
            public InputBuilder AssingmentFails
            {
                get
                {
                    var existingTrySet = _descriptor.TrySetValue;
                    _descriptor.TrySetValue = (cmd, val) => existingTrySet(cmd, val) & false;
                    return this;
                }
            }
            public ICommandInputDescriptor Descriptor
            {
                get { return _descriptor; }
            }

            public InputBuilder Position(int position)
            {
                _descriptor.Position = position;
                return this;
            }
        }

    }
}
