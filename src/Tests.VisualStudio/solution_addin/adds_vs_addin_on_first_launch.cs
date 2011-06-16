using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Core;
using OpenWrap.IO;
using OpenWrap.Testing;
using OpenWrap.VisualStudio;
using OpenWrap.VisualStudio.Hooks;
using OpenWrap.VisualStudio.SolutionAddIn;
using Tests;

namespace Tests.VisualStudio.solution_addin
{
    public class adds_vs_addin_on_first_launch : contexts.visual_studio
    {
        public adds_vs_addin_on_first_launch()
        {
            given_empty_solution_addin_com_reg();
            given_solution_file("mySolution.sln");
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod(OpenFileSystem.IO.IFile file) {} }");
            
            given_command("init-wrap . -name MyProject -all");

            when_building_with_vs2010(dte => dte.Solution.SaveAll(true));
        }

        [Test]
        public void solution_addin_is_added()
        {
            SlnFile.ReadString().ShouldContain(ComConstants.ADD_IN_PROGID_2010);
        }

        [Test]
        public void solution_addin_is_loaded()
        {
            Dte.Solution.AddIns.Update();
            Dte.Solution.AddIns.OfType<AddIn>().Where(x => x.ProgID == ComConstants.ADD_IN_PROGID_2010).ShouldHaveCountOf(1);
        }

        [Test]
        public void start_solutionplugin_command_loaded()
        {
            Output.ShouldContain(StartSolutionPlugin.SOLUTION_PLUGIN_STARTED);
        }
    }

}