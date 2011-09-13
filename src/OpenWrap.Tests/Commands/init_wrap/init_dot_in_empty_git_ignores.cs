using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using NUnit.Framework;

namespace Tests.Commands.init_wrap
{
    public class init_dot_in_empty_git_ignores : contexts.init_wrap
    {
        public init_dot_in_empty_git_ignores()
        {
            given_current_directory(@"c:\newpackage");
            given_project_repository(new FolderRepository(Environment.CurrentDirectory.GetDirectory("wraps"), FolderRepositoryOptions.AnchoringEnabled));
            when_executing_command(". -Git");
            Environment.ProjectRepository.RefreshPackages();
        }

        [Test]
        public void creates_file_in_wraps_dir()
        {
            Environment.CurrentDirectory.GetDirectory("wraps").GetFile(".gitignore").Exists.ShouldBeTrue();
            Environment.CurrentDirectory.GetFile(".gitignore").Exists.ShouldBeFalse();
        }

        [Test]
        public void does_not_use_glob_syntax()
        {
            IgnoreFileLines.ShouldNotContain("syntax: glob");
        }

        [Test]
        public void contains_cache_with_children()
        {
            var lines = IgnoreFileLines;
            lines.ShouldContain("_cache");
            lines.ShouldContain("_cache\\*");
        }

        string[] IgnoreFileLines
        {
            get { return AllLinesOf(Environment.CurrentDirectory.GetDirectory("wraps").GetFile(".gitignore")); }
        }
    }
}