namespace OpenWrap.Commands
{
    public static class CommandOutputExtensions
    {
        public static bool Success(this ICommandOutput output)
        {
            return output.Type != CommandResultType.Error;
        }
    }
}