using System;


namespace OpenWrap.Commands.Cli
{

    public static class ConsoleEx
    {
        public static ConsoleColor ProgressTextBackground = ConsoleColor.DarkCyan;
        public static ConsoleColor ProgressTextForeground = ConsoleColor.White;
        public static ConsoleColor ProgressBarForeground = ConsoleColor.Blue;
        public static ConsoleColor ProgressBarBackground = ConsoleColor.Black;
        static Action<string, string, int> ProgressHandler;
        static readonly object _syncLock = new object();
        static ConsoleEx()
        {
            if (EnvironmentDetection.IsCursorMoveAvailable && EnvironmentDetection.IsConsoleMoveBufferAvailable && EnvironmentDetection.IsConsoleResizeBufferAvailable)
                ProgressHandler = new ConsoleProgressOverlay().WriteProgress;
            //else if (canMoveCursor)
            //    ProgressHandler = new ConsoleProgressInline().WriteProgress;
            else
                ProgressHandler = (id, text, progress) => { if (progress > 200) Console.WriteLine(text); };
        }

        public static void WriteProgress(string id, string text, int progress)
        {
            lock(_syncLock)
            {
                ProgressHandler(id, text, progress);
            }
        }

        public static IDisposable Color(
            ConsoleColor? foreground = null,
            ConsoleColor? background = null)
        {
            var originalForeground = Console.ForegroundColor;
            var originalBackground = Console.BackgroundColor;
            if (foreground != null)
                Console.ForegroundColor = foreground.Value;
            if (background != null)
                Console.BackgroundColor = background.Value;

            return new ActionOnDispose(() =>
            {
                if (foreground != null)
                    Console.ForegroundColor = originalForeground;
                if (background != null)
                    Console.BackgroundColor = originalBackground;
            });
        }

        public static class ColoredText
        {
            public static IDisposable Gray
            {
                get { return Color(ConsoleColor.Gray); }
            }

            public static IDisposable Red
            {
                get { return Color(ConsoleColor.Red); }
            }

            public static IDisposable Yellow
            {
                get { return Color(ConsoleColor.Yellow); }
            }
        }

        public static IDisposable Position(int left, int top)
        {
            var previousTop = Console.CursorTop;
            var previousLeft = Console.CursorLeft;
            Console.SetCursorPosition(left, top);

            return new ActionOnDispose(() => Console.SetCursorPosition(previousLeft, previousTop));
        }
    }
}