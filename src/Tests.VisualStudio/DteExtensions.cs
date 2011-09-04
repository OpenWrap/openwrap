using System;
using System.Linq;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;

namespace Tests.VisualStudio
{
    public static class DteExtensions
    {
        public static void Close(this Solution sol, CloseOptions options)
        {
            var save = (options & CloseOptions.Save) == CloseOptions.Save;
            var wait = (options & CloseOptions.Wait) == CloseOptions.Wait;
            sol.Close(save);
            if (wait) WaitFor(null, () => sol.IsOpen == false, waitFor: TimeSpan.FromSeconds(20));
        }

        public static bool Contains(this string source, string message, int count)
        {
            return Regex.Matches(source, Regex.Escape(message)).Count == count;
        }

        public static OutputWindowPane Output(this Windows windows, string name, bool create = false)
        {
            var output = windows.Item(Constants.vsWindowKindOutput).Object as OutputWindow;
            if (output == null) return null;
            var pane = output.OutputWindowPanes.OfType<OutputWindowPane>().FirstOrDefault(x => x.Name == name);
            if (create && pane == null)
                pane = output.OutputWindowPanes.Add(name);
            return pane;
        }

        public static string Read(this OutputWindowPane pane)
        {
            if (pane == null) return null;

            var text = pane.TextDocument;
            text.Selection.StartOfDocument();
            text.Selection.EndOfDocument(true);
            return text.Selection.Text;
        }

        public static void SaveAll(this Solution sol, bool wait)
        {
            sol.DTE.ExecuteCommand("File.SaveAll");
            if (wait == false) return;
            while (sol.IsDirty || sol.Projects.OfType<Project>().Any(_ => _.IsDirty))
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(50));
        }

        public static void WaitFor(this DTE2 dte, Func<bool> predicate, TimeSpan waitFor = new TimeSpan())
        {
            if (waitFor == TimeSpan.Zero) waitFor = TimeSpan.FromMinutes(1);
            var start = DateTime.Now;
            while (predicate() == false)
            {
                if (DateTime.Now - start > waitFor)
                    throw new InvalidOperationException("Timed out while waiting on condition.");
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
        public static void WaitForOutputMessage(this DTE2 dte, string outputWindowName, string message, int messageCount)
        {
            dte.WaitFor(() =>
            {
                var output = dte.Windows.Output(outputWindowName);
                return output != null && output.Read().Contains(message, messageCount);
            }, TimeSpan.FromMinutes(1));
        }
        public static void WaitForMessage(this OutputWindowPane outputWindow, string message, int messageCount = 1, TimeSpan waitFor = new TimeSpan())
        {
            if (waitFor == TimeSpan.Zero) waitFor = TimeSpan.FromMinutes(1);
            ((DTE2)outputWindow.DTE).WaitFor(() => outputWindow.Read().Contains(message, messageCount), waitFor);
        }
    }
}