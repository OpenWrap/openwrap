using System;

namespace OpenWrap.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UICommandAttribute : Attribute
    {
        public UICommandContext Context { get; set; }
        public string Label { get; set; }
    }
}