namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteAdded : Info
    {
        public string Name { get; set; }

        public RemoteAdded(string name)
            : base("Remote repository '{0}' added.",  name)
        {
            Name = name;
        }
    }
}