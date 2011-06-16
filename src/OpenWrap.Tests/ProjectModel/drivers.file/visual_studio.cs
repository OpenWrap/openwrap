using System;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.IO;
using OpenWrap.ProjectModel;
using OpenWrap.ProjectModel.Drivers.File;
using OpenWrap.Testing;

namespace Tests.ProjectModel.drivers.file
{
    public class visual_studio : context
    {
        InMemoryFileSystem FileSystem;
        protected ITemporaryDirectory SlnDir;
        protected IFile SlnFile;
        protected ISolution Solution;
        protected string SlnContent;

        public visual_studio()
        {
            FileSystem = new InMemoryFileSystem();
            SlnDir = FileSystem.CreateTempDirectory();
        }
        protected void given_solution_file(string fileName, string content)
        {
            SlnFile = SlnDir.GetFile(fileName);
            SlnFile.WriteString(content);
        }

        protected void when_reading_solution()
        {
            Solution = OpenWrap.ProjectModel.Drivers.File.SolutionFile.Parse(SlnFile);
        }

        protected void when_writing_solution()
        {
            Solution.Save();
            SlnContent = SlnFile.ReadString();
            Solution = SolutionFile.Parse(SlnFile);
        }

        protected void given_solution(string solutionFile, Version vsVersion, bool openwrapAddin = false)
        {
            SlnFile = SlnDir.GetFile(solutionFile);
            Solution = new SolutionFile(SlnFile, vsVersion);
            Solution.OpenWrapAddInEnabled = openwrapAddin;
        }
    }
}