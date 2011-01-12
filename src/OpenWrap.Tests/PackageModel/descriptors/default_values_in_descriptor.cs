using NUnit.Framework;
using OpenWrap.PackageModel.descriptors.contexts;
using OpenWrap.Testing;

namespace package_descriptor_specs
{
    public class default_values_in_descriptor : descriptor
    {
        public default_values_in_descriptor()
        {
            given_descriptor();
        }

        [Test]
        public void use_symlink_is_true()
        {
            Descriptor.UseSymLinks.ShouldBeTrue();
        }

        [Test]
        public void anchor_is_false()
        {
            Descriptor.Anchored.ShouldBeFalse();
        }

        [Test]
        public void build_should_be_empty()
        {
            Descriptor.Build.ShouldBeEmpty();

        }

        [Test]
        public void use_project_repository_should_be_true()
        {
            Descriptor.UseProjectRepository.ShouldBeTrue();
        }
    }
}