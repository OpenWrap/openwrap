extern alias resharper450;
extern alias resharper500;
extern alias resharper510;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;
using OpenWrap.VisualStudio.Resharper;
using Tests;

namespace Tests.VisualStudio.resharper
{
    [Category("ReSharper")]
    public class rehsarper_plugin_manager_loads : contexts.visual_studio
    {
        public rehsarper_plugin_manager_loads()
        {
            given_openwrap_assemblyOf<ResharperLoaderPlugin>("solution");
            given_openwrap_assemblyOf<resharper450::OpenWrap.Resharper.PluginManager>("solution");
            given_openwrap_assemblyOf<resharper500::OpenWrap.Resharper.PluginManager>("solution");
            given_openwrap_assemblyOf<resharper510::OpenWrap.Resharper.PluginManager>("solution");

            given_solution_addin_com_reg();
            given_solution_file("mySolution.sln", true);
            given_project_2010("MyProject");
            given_file("Class1.cs", "public class ClassName { public static void MainMethod() {} }");

            given_command("init-wrap . -name MyProject -all");
            given_plugins_started();

            when_executing_vs2010(dte=>dte.Solution.SaveAll(true));
        }

        [Test]
        public void resharper_loader_loaded()
        {
            Output.ShouldContain("Resharper Plugin Manager loaded");
            Console.Write(Output);

        }
    }
}