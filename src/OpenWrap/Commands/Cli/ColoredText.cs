using System;

namespace OpenWrap.Commands.Cli
{
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

        static IDisposable Color(ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            return new ActionOnDispose(() => Console.ForegroundColor = originalColor);
        }
    }
}