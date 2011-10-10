namespace OpenWrap.Commands.Wrap
{
    public class NotInProject : Error
    {
        public NotInProject() : base("The current path was not recognized as being in an OpenWrap project or is missing a wrap descriptor.")
        {
        }
    }
}