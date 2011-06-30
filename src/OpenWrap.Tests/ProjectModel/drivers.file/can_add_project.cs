using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.IO;
using OpenWrap.ProjectModel.Drivers.File;
using OpenWrap.Testing;
using Tests;

namespace Tests.ProjectModel.drivers.file
{
    public class can_add_project : visual_studio
    {
        Guid _projectGuid;
        IFile _projectFile;

        public can_add_project()
        {
            given_solution("solution.sln", SolutionConstants.VisualStudio2010Version);
            _projectFile = SlnDir.GetDirectory("Project").GetFile("Project.csproj");
            _projectGuid = given_project_file(_projectFile);
            when_adding_project(_projectFile);
        }

        [Test]
        public void solution_file_is_generated_correctly()
        {
            SlnFile.ReadString().ShouldBe("\r\nMicrosoft Visual Studio Solution File, Format Version 11.00\r\n" +
                                          "# Visual Studio 2010\r\n" +
                                          string.Format("Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"\r\n",
                                                        ProjectConstants.GuidFromType("csharp").ToString().ToUpper(),
                                                        _projectFile.NameWithoutExtension,
                                                        _projectFile.Path.MakeRelative(SlnFile.Parent.Path),
                                                        _projectGuid.ToString().ToUpper()) +
                                          "EndProject\r\n");
        }
        void when_adding_project(IFile projectFile)
        {
            Solution.AddProject(projectFile);
            Solution.Save();
        }

        Guid given_project_file(IFile getFile, string @namespace="OpenWrap.Tests", string assemblyName="openwrap.tests")
        {
            var projectGuid = Guid.NewGuid();
            getFile.WriteString(string.Format(Files.project_classlib, projectGuid, @namespace, assemblyName, "bin/Debug"));
            return projectGuid;
        }
    }
}