using System;

namespace OpenWrap.Commands
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandInputAttribute : Attribute
    {
        public CommandInputAttribute()
        {
            Position = -1;
            IsValueRequired = true;
        }

        public string Description { get; set; }
        public string Label { get; set; }

        public bool IsRequired { get; set; }
        public bool IsValueRequired { get; set; }

        public string Name { get; set; }
        public int Position { get; set; }
    }
}