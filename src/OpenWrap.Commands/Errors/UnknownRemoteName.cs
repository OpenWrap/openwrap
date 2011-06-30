namespace OpenWrap.Commands.Errors
{
    public class UnknownRemoteName : Error
    {
        public string Name { get; set; }

        public UnknownRemoteName(string name)
            : base("Unknown remote repository '{0}'. To see the list of your remote repositories, use the 'list-remote' command.", name)
        {
            Name = name;
        }
    }
}