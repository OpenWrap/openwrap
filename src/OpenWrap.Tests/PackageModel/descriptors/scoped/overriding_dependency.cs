using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.PackageModel.descriptors.scoped
{
    public class overriding_dependency : scoped_descriptor
    {
        public overriding_dependency()
        {
            given_descriptor("name: one-ring", "depends: sauron = 2.0.0");
            given_scoped_descriptor();
            ScopedDescriptor.Dependencies.Add(new PackageDependencyBuilder("sauron").Version("1.0.0"));

            when_writing();
        }

        [Test]
        public void only_one_named_dependency_exists()
        {
            ScopedDescriptor.Dependencies.Where(x => x.Name.EqualsNoCase("sauron")).ShouldHaveCountOf(1);
        }
        [Test]
        public void dependency_in_default_is_preserved()
        {
            descriptor_should_be("name: one-ring", "depends: sauron = 2.0.0");
        }

        [Test]
        public void dependency_in_scope_is_modified()
        {
            scoped_descriptor_should_be("depends: sauron = 1.0.0");
        }
    }
}