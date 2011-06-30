using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using EnvDTE;
using EnvDTE80;

namespace OpenWrap.VisualStudio
{
    public static class DteGuard
    {
        static Queue<Action<DTE2>> _callQueue = new Queue<Action<DTE2>>();
        static System.Threading.Thread _dteQueue;
        static DteGuard()
        {
            _dteQueue = new System.Threading.Thread(() =>
            {
                while (!TryExecuteNow(dte => { }))
                {
                    Trace.WriteLine("DTE not available, sleeping for a while.");
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }) { Name = "OpenWrap DTE calls"};
            _dteQueue.SetApartmentState(ApartmentState.STA);
        }
        public static void BeginInvoke(Action<DTE2> dteAction)
        {
            if (!TryExecuteNow(dteAction))
                _dteQueue.Start();
        }

        static bool TryExecuteNow(Action<DTE2> dteAction)
        {
            DTE2 dte = null;
            try
            {
                dte = SiteManager.GetGlobalService<DTE>() as DTE2;
            }
            catch(Exception e)
            {
                Debug.WriteLine("Failed to get DTE because " + e.ToString());

            }
            if (dte == null)
            {
                lock(_callQueue)
                {
                    _callQueue.Enqueue(dteAction);
                    return false;
                }
            }
            lock(_callQueue)
            {
                while(_callQueue.Count > 0)
                {
                    RunTask(dte, _callQueue.Dequeue());
                }
            }
            RunTask(dte, dteAction);
            return true;
        }

        static void RunTask(DTE2 dte, Action<DTE2> dequeue)
        {
            try
            {
                dequeue(dte);
            }
            catch(Exception e)
            {
                Trace.WriteLine("Failed to run action:\r\n" + e.ToString());
            }
        }
    }
}