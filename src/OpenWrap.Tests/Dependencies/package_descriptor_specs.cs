using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace package_descriptor_specs
{
    public class removing_a_multiline_value : contexts.descriptor
    {
        public removing_a_multiline_value()
        {
            given_descriptor("depends: ered-luin", "depends: ered-mithrin", "depends: ered-lithui");
            Descriptor.Dependencies.Remove(Descriptor.Dependencies.First(x => x.Name.EqualsNoCase("ered-mithrin")));

            when_writing();
        }
        [Test]
        public void order_of_remaining_lines_is_preserved()
        {
            should_have_content("depends: ered-luin\r\ndepends: ered-lithui");
        }

    }
    
    public class adding_a_multiline_value : contexts.descriptor
    {
        public adding_a_multiline_value()
        {
            given_descriptor("depends: ered-luin");
            Descriptor.Dependencies.Add(new PackageDependency("ered-mithrin"));

            when_writing();
        }

        [Test]
        public void value_is_appended()
        {
            should_have_content("depends: ered-luin", "depends: ered-mithrin");
        }
    }
    public class editing_single_value : contexts.descriptor
    {
        public editing_single_value()
        {
            given_descriptor("anchored: false", "build: value");
            Descriptor.Anchored = true;
            when_writing();
        }

        [Test]
        public void value_is_edited_in_place()
        {
            should_have_content("anchored: true\r\nbuild: value");
        }
    }
    public class setting_single_value_to_default : contexts.descriptor
    {
        public setting_single_value_to_default()
        {
            given_descriptor("anchored: true");
            Descriptor.Anchored = false;
            when_writing();
        }

        [Test]
        public void line_is_removed()
        {
            should_have_content("");
        }
    }
    public class unknown_lines_in_descriptor : contexts.descriptor
    {
        public unknown_lines_in_descriptor()
        {
            given_descriptor("anchored: false", "my-custom-value: something", "build: value");
            Descriptor.Anchored = true;
            Descriptor.BuildCommand = "newValue";
            when_writing();
        }

        [Test]
        public void value_is_preserved()
        {
            should_have_content("anchored: true", "my-custom-value: something", "build: newValue");
        }
    }
    public class default_values_in_descriptor : contexts.descriptor
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
        public void build_should_be_null()
        {
            Descriptor.BuildCommand.ShouldBeNull();

        }

        [Test]
        public void use_project_repository_should_be_true()
        {
            Descriptor.UseProjectRepository.ShouldBeTrue();
        }
    }
    public class setting_symlinks : contexts.descriptor
    {
        public setting_symlinks()
        {
            given_descriptor();
            Descriptor.UseSymLinks = false;
            when_writing();
        }
        [Test]
        public void symlinks_are_disabled()
        {
            should_have_content("use-symlinks: false");
        }
    }
    namespace contexts
    {
        public abstract class descriptor : context
        {
            protected PackageDescriptor Descriptor;
            protected string Content;

            protected void when_writing()
            {
                Content = Descriptor.Select(x => x.Name + ": " + x.Value).Join("\r\n") ?? string.Empty;

            }
            protected void should_have_content(params string[] expectedContent)
            {
                Content.ShouldBe(expectedContent.Join("\r\n") ?? string.Empty);
            }
            protected void given_descriptor(params string[] lines)
            {
                Descriptor = new PackageDescriptorReaderWriter().Read(new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\r\n", lines))));
            }
        }
    }
}
