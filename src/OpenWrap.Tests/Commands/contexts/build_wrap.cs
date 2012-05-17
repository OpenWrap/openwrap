using OpenFileSystem.IO;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;

namespace Tests.Commands.contexts
{
    public class build_wrap : command<BuildWrapCommand>
    {
        protected void given_descriptor(IDirectory projectDirectory, PackageDescriptor packageDescriptor)
        {
            using(var descriptor = projectDirectory.GetFile(packageDescriptor.Name + ".wrapdesc").OpenWrite())
                new PackageDescriptorWriter().Write(packageDescriptor, descriptor);
            Environment.Descriptor = packageDescriptor;
        }
    }
}