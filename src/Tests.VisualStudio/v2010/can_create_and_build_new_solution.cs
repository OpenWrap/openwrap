using NUnit.Framework;
using OpenWrap.SolutionPlugins.VisualStudio.ReSharper;
using OpenWrap.Testing;
using Tests.VisualStudio.contexts;

namespace Tests.VisualStudio.v2010
{
    public class can_create_and_build_new_solution : visual_studio
    {
        public can_create_and_build_new_solution()
        {
            given_openwrap_assemblyOf<ReSharperLoaderPlugin>("solution");

            given_solution_file("mySolution.sln");
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod(OpenFileSystem.IO.IFile file) {} }");
            given_command("init-wrap . -name MyProject -all");
            given_command("add-wrap openfilesystem");
            given_vs_action(dte => dte.Solution.SolutionBuild.Build(true), dte => dte.ExecuteCommand("File.SaveAll"));

            when_executing_vs2010();
        }

        [Test]
        public void assembly_is_compiled()
        {
            OutDir.GetFile("MyProject.dll").Exists.ShouldBeTrue();
        }

        [Test]
        public void build_succeeds()
        {
            Errors.ShouldBeEmpty();
        }
    }
}