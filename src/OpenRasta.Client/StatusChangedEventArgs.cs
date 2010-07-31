using System;

namespace OpenRasta.Client
{
    public class StatusChangedEventArgs : EventArgs
    {
        public StatusChangedEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}