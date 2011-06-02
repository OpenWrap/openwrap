namespace OpenWrap.Commands.Remote
{
    public class RemoteNameInvalid : Error
    {
        public RemoteNameInvalid() : base("The 'Name' parameter is invalid for a remote name. Identifiers cannot contain spaces.")
        {
        }
    }
}