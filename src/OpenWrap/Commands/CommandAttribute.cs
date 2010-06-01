using System;

namespace OpenWrap.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string Noun { get; set; }
        public string Verb { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}