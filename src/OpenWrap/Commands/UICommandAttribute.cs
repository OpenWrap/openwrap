using System;

namespace OpenWrap.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited =false)]
    public class UICommandAttribute : Attribute
    {
        public string Label { get; set; }
        public UICommandContext Context { get; set; }
    }
}