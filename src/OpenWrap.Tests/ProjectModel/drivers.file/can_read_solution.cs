using System;
using NUnit.Framework;
using OpenWrap.ProjectModel.Drivers.File;
using OpenWrap.Testing;

namespace Tests.ProjectModel.drivers.file
{
    public class can_read_solution : visual_studio
    {
        public can_read_solution()
        {
            given_solution_file(@"solution.sln",
                                 "Microsoft Visual Studio Solution File, Format Version 11.00\r\n" +
                                 "# Visual Studio 2010\r\n" +
                                @"Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""Tests.VisualStudio"", ""Tests.VisualStudio\Tests.VisualStudio.csproj"", ""{E24298AF-B37B-4DCB-9F7E-1C4A85DBC821}""
EndProject\r\n");
            when_reading_solution();
        }

        [Test]
        public void solution_has_correct_version()
        {
            Solution.Version.ShouldBe(new Version(11, 0));
        }
        [Test]
        public void solution_has_one_project()
        {
            Solution.AllProjects.ShouldHaveCountOf(1);
        }
    }
}