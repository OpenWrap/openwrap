using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.VisualStudio.v2010
{
    public class can_create_and_build_new_solution : contexts.visual_studio
    {
        public can_create_and_build_new_solution()
        {
            given_solution_file("mySolution.sln");
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod(OpenFileSystem.IO.IFile file) {} }");
            given_command("init-wrap . -name MyProject -all");
            given_command("add-wrap openfilesystem");
            when_building_with_vs2010(dte=> dte.Solution.SolutionBuild.Build(true), dte=> dte.ExecuteCommand("File.SaveAll"));
        }

        [Test]
        public void build_succeeds()
        {
            Errors.ShouldBeEmpty();
        }

        [Test]
        public void assembly_is_compiled()
        {
            OutDir.GetFile("MyProject.dll").Exists.ShouldBeTrue();
        }

    }

}