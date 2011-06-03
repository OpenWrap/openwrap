namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteRemoved : Info
    {
        public string Name { get; set; }

        public RemoteRemoved(string name)
            : base("Repository '{0}' removed.", name)
        {
            Name = name;
        }
    }
}