namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteNameInvalid : Error
    {
        public string Name { get; set; }

        public RemoteNameInvalid(string name) : base("The value '{0}' is not a valid remote name. Identifiers cannot contain spaces.", name)
        {
            Name = name;
        }
    }
}