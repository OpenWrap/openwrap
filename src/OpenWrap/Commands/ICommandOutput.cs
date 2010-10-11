using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public interface ICommandOutput
    {
        ICommand Source { get; }
        CommandResultType Type { get; }
    }
    public static class CommandOutputExtensions
{
    public static bool Success(this ICommandOutput output)
    {
        return output.Type != CommandResultType.Error;
    }
}
}