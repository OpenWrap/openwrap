using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.Testing;

namespace OpenWrap.Tests
{
    public static class CommandsExtensions
    {
        public static IList<ICommandOutput> ShouldContain<T>(this IList<ICommandOutput> results) where T: ICommandOutput
        {
            results.Any(x => typeof(T).IsAssignableFrom(x.GetType())).ShouldBeTrue();
            return results;
        }
    }
}
