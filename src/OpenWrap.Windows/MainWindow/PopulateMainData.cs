using System;
using System.Collections.Generic;
using System.Linq;

using OpenWrap.Collections;
using OpenWrap.Commands;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.NounVerb;

namespace OpenWrap.Windows.MainWindow
{
    public class PopulateMainData : CommandBase<MainViewModel>
    {
        protected override void Execute(MainViewModel parameter)
        {
            // todo: constructor-inject some services
            var commands = Services.Services.GetService<ICommandRepository>();
            var nouns = commands != null ? RealCommands(commands) : MockCommands();

            parameter.Nouns.Clear();
            parameter.Nouns.AddRange(nouns);
        }

        private static NounSlice CreateNounSlice(IGrouping<string, ICommandDescriptor> x)
        {
            if (x.Key.Equals("wrap", StringComparison.OrdinalIgnoreCase))
                return new WrapSlice(x.Key, x.Select(y => new VerbSlice(y)));
            return new NounSlice(x.Key, x.Select(y => new VerbSlice(y)));
        }

        private static IEnumerable<NounSlice> RealCommands(IEnumerable<ICommandDescriptor> commands)
        {
            return commands.GroupBy(x => x.Noun).Select(CreateNounSlice);
        }

        private static IEnumerable<NounSlice> MockCommands()
        {
            yield return new NounSlice("Test 1", new[] { new VerbSlice(new NullCommandDescriptor()) });
            yield return new NounSlice("Test 2", new[] { new VerbSlice(new NullCommandDescriptor()) });
        }
    }
}
