namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteUpdated : Info
    {
        public string Name { get; set; }

        public RemoteUpdated(string name) : base("Remote '{0}' updated.", name)
        {
            Name = name;
        }
    }
}