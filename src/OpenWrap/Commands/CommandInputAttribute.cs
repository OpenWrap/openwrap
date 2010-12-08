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
        public bool IsRequired
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public string DisplayName
        {
            get;
            set;
        }
        public int Position
        {
            get; set;
        }

        public bool IsValueRequired { get; set; }
    }
}