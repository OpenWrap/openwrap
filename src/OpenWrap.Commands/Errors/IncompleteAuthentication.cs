namespace OpenWrap.Commands.Errors
{
    public class IncompleteAuthentication : Error
    {
        public override string ToString()
        {
            return "When using authentication you must provide both 'user' and 'pwd' arguments";
        }
    }
}