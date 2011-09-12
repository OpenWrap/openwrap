using System;
using System.IO;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using NUnit.Framework;

namespace Tests.Commands.init_wrap
{
    public class init_dot_in_empty_mercurial_ignores : contexts.init_wrap
    {
        public init_dot_in_empty_mercurial_ignores()
        {
            given_current_directory(@"c:\newpackage");
            given_project_repository(new FolderRepository(Environment.CurrentDirectory.GetDirectory("wraps"), FolderRepositoryOptions.AnchoringEnabled));
            when_executing_command(". -Hg");
            Environment.ProjectRepository.RefreshPackages();
        }

        [Test]
        public void creates_file_in_project_root()
        {
            Environment.CurrentDirectory.GetFile(".hgignore").Exists.ShouldBeTrue();
            Environment.CurrentDirectory.GetDirectory("wraps").GetFile(".hgignore").Exists.ShouldBeFalse();
        }

        [Test]
        public void uses_glob_syntax()
        {
            IgnoreFileLines.ShouldContain("syntax: glob");
        }

        [Test]
        public void contains_wraps_cache()
        {
            IgnoreFileLines.ShouldContain("wraps/_cache");
        }

        string[] IgnoreFileLines
        {
            get { return AllLinesOf(Environment.CurrentDirectory.GetFile(".hgignore")); }
        }
    }
}
