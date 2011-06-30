namespace OpenWrap.Commands.Messages
{
    public class IncompleteCredentials : Error
    {
        public IncompleteCredentials()
            : base("When providing credentials, both a username and a password are needed.")
        {   
        }
    }
}