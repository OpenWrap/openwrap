using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;

namespace OpenWrap.Commands.contexts
{
    public abstract class remove_wrap_command : command_context<RemoveWrapCommand>
    {

        protected IPackageDescriptor PostCommandDescriptor;
        protected override void when_executing_command(string parameters)
        {
            base.when_executing_command(parameters);
            PostCommandDescriptor = new PackageDescriptorReaderWriter().Read(Environment.DescriptorFile);
        }
    }
}