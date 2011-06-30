namespace OpenWrap.Commands.Remote.Messages
{
    public class UnknownEndpointType : Error
    {
        public string Path { get; set; }

        public UnknownEndpointType(string path)
            : base("The address '{0}' was not recognized as a known repository type.", path)
        {
            Path = path;
        }
    }
}