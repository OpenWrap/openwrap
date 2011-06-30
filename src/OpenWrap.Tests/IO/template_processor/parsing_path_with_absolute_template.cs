using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO.template_processor
{
    [TestFixture(@"{source: src}")]
    [TestFixture(@"**\{source: src}")]
    public class finding_one_directory : contexts.template_processor
    {
        public finding_one_directory(string template)
        {
            given_current_directory(@"c:\projects\MyProject\");
            given_template(template);
            given_directory(@"c:\projects\MyProject\src\MyProject");

            when_finding_directories();
        }

        [Test]
        public void directory_is_found()
        {
            Directories.ShouldHaveCountOf(1);
            Directories.Single().Item.Path.FullPath.ShouldBe(@"c:\projects\MyProject\src\");
        }

        [Test]
        public void parameters_are_correct()
        {
            Directories.Single().Parameters.ShouldHaveCountOf(1)
                    .First().ShouldBe("source", "src");
        }
    }
    [TestFixture(@"c:\**\{source: src}")]
    [TestFixture(@"c:\projects\**\{source: src}")]
    public class parsing_path_with_absolute_template : contexts.template_processor
    {
        public parsing_path_with_absolute_template(string template)
        {
            given_template(template);

            when_parsing_path(@"c:\projects\src\dir\project.proj");
        }

        [Test]
        public void has_one_property()
        {
            Properties.Count.ShouldBe(1);
        }

        [Test]
        public void property_value_is_name_of_folder()
        {
            Properties["source"].ShouldBe("src");
        }
    }
    [TestFixture(@"projects\**\{source: src}\**")]
    [TestFixture(@"projects\{source: src}")]
    [TestFixture(@"**\{source: src}")]
    public class parsing_path_with_relative_template : contexts.template_processor
    {
        public parsing_path_with_relative_template(string template)
        {
            given_template(template);

            when_parsing_path(@"c:\projects\src\dir\project.proj");
        }

        [Test]
        public void has_one_property()
        {
            Properties.Count.ShouldBe(1);
        }

        [Test]
        public void property_value_is_name_of_folder()
        {
            Properties["source"].ShouldBe("src");
        }
    }
    public class parsing_path_with_relative_template_repeating_segments : contexts.template_processor
    {
        public parsing_path_with_relative_template_repeating_segments()
        {
            given_template(@"src\{scope: project}\**");
            when_parsing_path(@"c:\src\middle-earth\src\project\project.csproj");
        }

        [Test]
        public void property_is_found()
        {
            Properties["scope"].ShouldBe("project");
        }
    }
    namespace contexts
    {
        public abstract class template_processor : OpenWrap.Testing.context
        {
            protected InMemoryFileSystem FileSystem;
            PathTemplateProcessor Processor;
            protected IDictionary<string, string> Properties;
            bool SuccessfulParsing;
            protected List<PathTemplateItem<IDirectory>> Directories;

            public template_processor()
            {
                FileSystem = new InMemoryFileSystem();
            }

            protected void given_directory(string dir)
            {
                FileSystem.GetDirectory(dir).MustExist();
            }

            protected void given_template(string path)
            {
                Processor = new PathTemplateProcessor(path);
            }

            protected void when_parsing_path(string path)
            {
                IDictionary<string,string> props;
                SuccessfulParsing = Processor.TryParsePath(new Path(path), false, out props);
                Properties = props;
            }

            protected void given_file(string path)
            {
                FileSystem.GetFile(path).MustExist();
            }

            protected void given_current_directory(string path)
            {
                FileSystem.CurrentDirectory = path;
            }

            protected void when_finding_directories()
            {
                Directories = Processor.Directories(FileSystem.GetCurrentDirectory()).ToList();
            }
        }
    }
}
