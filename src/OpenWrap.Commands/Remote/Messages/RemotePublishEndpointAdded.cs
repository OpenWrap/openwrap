namespace OpenWrap.Commands.Remote.Messages
{
    public class RemotePublishEndpointAdded : Info
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public RemotePublishEndpointAdded(string name, string path)
            : base("Publish endpoint '{0}' added to remote repository '{1}'.",path, name)
        {
            Name = name;
            Path = path;
        }
    }
}