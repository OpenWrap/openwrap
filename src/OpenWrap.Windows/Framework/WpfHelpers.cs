using System;
using System.Windows;
using System.Windows.Threading;

namespace OpenWrap.Windows.Framework
{
    public static class WpfHelpers
    {
        public static void CenterInParent(this Window window, Window parent)
        {
            window.Left = parent.Left + ((parent.Width - window.Width) / 2);
            window.Top = parent.Top + ((parent.Height - window.Height) / 2);
        }

        public static void DispatchToMainThread(Action action)
        {
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(action, DispatcherPriority.Normal);
        }
    }
}
