using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Testing;

namespace Tests.Commands.contexts
{
    public static class CommandOutputExtensions
    {
        public static T ShouldHaveOneOf<T>(this IEnumerable<ICommandOutput> output)
        {
            return output.OfType<T>().Check(x => x.ShouldHaveCountOf(1)).Single();
        }
    }
}