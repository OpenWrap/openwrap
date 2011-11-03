using NUnit.Framework;
using Tests.contexts;

namespace OpenWrap.PackageModel.descriptors.scoped
{
    public class adding_dependency : scoped_descriptor
    {
        public adding_dependency()
        {
            given_descriptor("name: one-ring");
            given_scoped_descriptor();
            ScopedDescriptor.Dependencies.Add(new PackageDependencyBuilder("sauron"));

            when_writing();
        }

        [Test]
        public void dependency_is_added_to_scoped()
        {
            scoped_descriptor_should_be("depends: sauron");
        }

        [Test]
        public void default_descriptor_not_modified()
        {
            descriptor_should_be("name: one-ring");
        }
    }
}