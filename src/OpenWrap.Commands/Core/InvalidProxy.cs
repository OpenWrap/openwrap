namespace OpenWrap.Commands.Core
{
    public class InvalidProxy : Error
    {
        public InvalidProxy(string href)
            : base("'{0}' was not recognized as a valid proxy URI.", href)
        {
            Href = href;
        }

        public string Href { get; set; }
    }
}