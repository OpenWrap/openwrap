using System;

namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteNameInUse : Error
    {
        public RemoteNameInUse(string name)
            : base("A repository with the name '{0}' already exists. Try specifying a different name.", name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}