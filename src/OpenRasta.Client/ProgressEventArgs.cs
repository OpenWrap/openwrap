using System;

namespace OpenRasta.Client
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int progress)
        {
            Progress = progress;
        }

        public int Progress { get; set; }
    }
}