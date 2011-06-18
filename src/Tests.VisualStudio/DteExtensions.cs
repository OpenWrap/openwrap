using System;
using System.Linq;
using System.Threading;
using EnvDTE;
using EnvDTE80;

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
        public static void WaitForMessage(this OutputWindowPane dte, string message)
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            var waitFor = TimeSpan.FromMinutes(1);
            var start = DateTime.Now;
            while (dte.Read().Contains(message) == false)
            {
                if (DateTime.Now - start > waitFor)
                    throw new InvalidOperationException("The message did not appear after a minute.");
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
        public static OutputWindowPane Output(this Windows windows, string name)
        {
            var output = windows.Item(Constants.vsWindowKindOutput).Object as OutputWindow;
            if (output == null) return null;
            return output.OutputWindowPanes.OfType<OutputWindowPane>().FirstOrDefault(x => x.Name == name);

        }
        public static string Read(this OutputWindowPane pane)
        {
            if (pane == null) return null;

            var text = pane.TextDocument;
            text.Selection.StartOfDocument();
            text.Selection.EndOfDocument(true);
            return text.Selection.Text;
        }
    }
}