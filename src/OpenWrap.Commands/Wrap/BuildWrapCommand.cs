using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="Wrap", Verb="Build")]
    public class BuildWrapCommand : AbstractCommand
    {
        protected IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); } }
        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(NoDescriptorFound).Or(Build());
        }
        ICommandOutput NoDescriptorFound()
        {
            return Environment.Descriptor == null ? new GenericError("Could not find a wrap descriptor. Are you in a project directory?") : null;
        }
        IEnumerable<ICommandOutput> Build()
        {
            var commandLine = Environment.Descriptor.BuildCommand;
            if (commandLine == null)
             foreach(var m in BuildWithConventionalMSBuild()) yield return m;
        }

        IEnumerable<ICommandOutput> BuildWithConventionalMSBuild()
        {
            yield break;
        }
    }
}
