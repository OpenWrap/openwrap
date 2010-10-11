using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.Testing;

namespace OpenWrap
{
    public static class CommandsExtension
    {
        public static IEnumerable<ICommandOutput> ShouldHaveNoError(this IEnumerable<ICommandOutput> results)
        {
            return results.ShouldHaveNo(x => x.Type == CommandResultType.Error);
        }
        public static IEnumerable<ICommandOutput> ShouldHaveError(this IEnumerable<ICommandOutput> results)
        {
            return results.ShouldHaveAtLeastOne(x => x.Type == CommandResultType.Error);
        }
    }
}
