using System;

namespace OpenWrap.Configuration
{
    public class KeyAttribute : Attribute
    {
        public KeyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}