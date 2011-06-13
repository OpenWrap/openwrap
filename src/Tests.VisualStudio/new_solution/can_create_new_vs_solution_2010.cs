using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using Tests;

namespace Tests.VisualStudio.new_solution
{
    public class can_create_new_vs_solution_2010 : contexts.visual_studio
    {
        public can_create_new_vs_solution_2010()
        {
            given_solution_file("mySolution.sln");
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod(OpenFileSystem.IO.IFile file) {} }");
            given_command("init-wrap . -name MyProject -all");
            given_command("add-wrap openfilesystem");
            when_building_with_vs2010();
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