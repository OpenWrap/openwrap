using System;
using System.Reflection;
using System.Text;
using System.Diagnostics;


namespace OpenWrap.Build.Tasks
{
    static class ResharperLogger
    {
        public static void Debug(string text, params string[] args)
        {
            Debugger.Log(0, "resharper", DateTime.Now.ToShortTimeString() + ":" + string.Format(text, args) + "\r\n");
        }
    }
}
