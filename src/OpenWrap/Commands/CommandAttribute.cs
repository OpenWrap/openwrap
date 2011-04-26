using System;

namespace OpenWrap.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Description { get; set; }
        public string Noun { get; set; }
        public string Verb { get; set; }
        public bool Visible { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited =false)]
    public class UICommandAttribute : Attribute
    {
        public string Label { get; set; }
        public UICommandContext Context { get; set; }
    }
}