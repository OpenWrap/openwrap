using System;

namespace OpenWrap
{
    public static class EnvironmentDetection
    {
        public static bool IsUnix
        {
            get 
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static bool IsConsoleKeyInteractive
        {
            get
            {
                if (!Environment.UserInteractive) return false;
                return Try(() => { var avail = Console.KeyAvailable; });
            }
        }

        static bool Try(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsConsoleMoveBufferAvailable
        {
            get
            {
                return Try(()=> Console.MoveBufferArea(0,0,1,1,0,0));
            }
        }
        public static bool IsConsoleResizeBufferAvailable
        {
            get
            {
                try
                {
                    Console.BufferHeight++;
                    Console.BufferHeight--;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static bool IsCursorMoveAvailable
        {
            get
            {
                return Try(() =>
                {
                    Console.CursorLeft++;
                    Console.CursorLeft--;
                });
            }
        }

    }
}