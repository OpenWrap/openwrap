namespace OpenWrap.Commands.Core
{
    public class EmptyConfiguration : Info
    {
        public EmptyConfiguration(string file)
            : base("No configuration specified in '{0}'.", file)

        {
            File = file;
        }

        public string File { get; set; }
    }
}