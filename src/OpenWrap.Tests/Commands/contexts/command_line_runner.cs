using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.Testing;

namespace Tests.Commands.contexts
{
    public abstract class command_line_runner : context
    {
        protected bool CommandExecuted;
        protected IEnumerable<ICommandOutput> Results;
        readonly Dictionary<string, List<string>> _properties;
        MemoryCommandDescriptor Command;
        List<string> _optionalInputs;
        protected MemoryCommand CommandInstance;

        public command_line_runner()
        {
            _properties = new Dictionary<string, List<string>>((StringComparer.OrdinalIgnoreCase));
            _optionalInputs = new List<string>();

        }

        protected IEnumerable<string> Input(string inputName)
        {
            return _properties.ContainsKey(inputName) ? _properties[inputName] : Enumerable.Empty<string>();
        }

        protected void given_command(string verb, string noun, params Expression<Func<InputBuilder, InputBuilder>>[] properties)
        {
            given_command<MemoryCommand>(verb, noun, properties);
        }
        protected void given_command<T>(string verb, string noun, params Expression<Func<InputBuilder, InputBuilder>>[] properties) where T:MemoryCommand,new()
        {
            Command = new MemoryCommandDescriptor(
                    properties.Select(property => property.Compile()(
                            new InputBuilder(property.Parameters[0].Name)
                                .Setter((cmd, val) => SaveProperty(property.Parameters[0].Name, val)))
                                .Descriptor))
            {
                    Verb = verb,
                    Noun = noun,
                    Create = () => CommandInstance = new T
                    {
                            Execute = () =>
                            {
                                CommandExecuted = true;
                                return Enumerable.Empty<ICommandOutput>();
                            }
                    }
            };
        }

        protected void given_optional_input(string inputName)
        {
            _optionalInputs.Add(inputName);
        }

        protected void when_executing(string line)
        {
            Results = new CommandLineRunner { OptionalInputs = _optionalInputs }.Run(Command, line).ToList();
        }

        bool SaveProperty(string name, IEnumerable<string> val)
        {
            _properties.GetOrCreate(name).AddRange(val);
            return true;
        }

        public class InputBuilder
        {
            readonly MemoryCommandInput _descriptor;

            public InputBuilder(string name)
            {
                _descriptor = new MemoryCommandInput { Name = name };
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

            public InputBuilder Required
            {
                get
                {
                    _descriptor.IsRequired = true;
                    return this;
                }
            }

            public InputBuilder Position(int position)
            {
                _descriptor.Position = position;
                return this;
            }

            public InputBuilder Setter(Func<ICommand, IEnumerable<string>, bool> setter)
            {
                _descriptor.TrySetValue = setter;
                return this;
            }

            public InputBuilder Type<T>()
            {
                _descriptor.Type = typeof(T).Name;
                return this;
            }
        }
    }
}