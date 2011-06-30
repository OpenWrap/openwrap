namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteEndpointReadOnly : Error
    {
        public string Path { get; set; }

        public RemoteEndpointReadOnly(string path)
            : base("The path '{0}' is not recognized as a repository that can be published to.", path)
        {
            Path = path;
        }
    }
}