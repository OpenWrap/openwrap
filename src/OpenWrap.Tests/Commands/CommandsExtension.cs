﻿using System;
using System.Collections.Generic;
using OpenWrap.Commands;
using OpenWrap.Testing;

namespace Tests.Commands
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

        public static IEnumerable<ICommandOutput> ShouldHaveWarning(this IEnumerable<ICommandOutput> results)
        {
            return results.ShouldHaveAtLeastOne(x => x.Type == CommandResultType.Warning);
        }
    }
}
