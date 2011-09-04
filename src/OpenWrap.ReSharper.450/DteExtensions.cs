using EnvDTE;

namespace OpenWrap.Resharper
{
    public static class DteExtensions
    {
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