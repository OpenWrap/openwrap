using System;
using System.Collections.Generic;
using OpenWrap.Commands;

namespace Tests.Commands
{
    public class MemoryCommandInput : ICommandInputDescriptor
    {
        public MemoryCommandInput()
        {
            IsValueRequired = true;
            TrySetValue = (target, value) => false;

        }
        public bool IsRequired { get; set; }

        public bool IsValueRequired { get; set; }

        public bool MultiValues { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? Position { get; set; }
        public string Type { get; set; }
        public Func<ICommand, IEnumerable<string>, bool> TrySetValue { get; set; }
        bool ICommandInputDescriptor.TrySetValue(ICommand target, IEnumerable<string> value)
        {
            
            return TrySetValue(target, value);
        }
    }
}