using System;

namespace OpenWrap.Commands.Remote.Messages
{
    public class UnknownRemoteName : Error
    {
        public string Name { get; set; }

        public UnknownRemoteName(string name)
        {
            Name = name;
        }
    }
}