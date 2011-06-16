using System;
using System.Linq;
using EnvDTE;

namespace Tests.VisualStudio
{
    public static class DteExtensions
    {
        public static void SaveAll(this Solution sol, bool wait)
        {
            sol.DTE.ExecuteCommand("File.SaveAll");
            if (wait == false) return;
            while (sol.IsDirty || sol.Projects.OfType<Project>().Any(_ => _.IsDirty))
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(50));
        }
    }
}