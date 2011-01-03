using System;
using System.Windows;

namespace OpenWrap.Windows.Framework
{
    public static class WpfHelpers
    {
        public static void CenterInParent(this Window window, Window parent)
        {
            window.Left = parent.Left + ((parent.Width - window.Width) / 2);
            window.Top = parent.Top + ((parent.Height - window.Height) / 2);
        }
    }
}
