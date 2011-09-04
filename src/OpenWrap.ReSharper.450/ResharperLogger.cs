using System;

namespace OpenWrap.Resharper
{
    internal static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            System.Diagnostics.Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");
        }
    }
}