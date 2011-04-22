using System;

namespace OpenWrap.Commands.Cli
{
    public static class ColoredText
    {
        public static IDisposable Red { get { return Color(System.ConsoleColor.Red); } }
        public static IDisposable Yellow { get { return Color(System.ConsoleColor.Yellow); } }
        public static IDisposable Gray { get { return Color(System.ConsoleColor.Gray); } }
        static IDisposable Color(System.ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            return new ActionOnDispose(() => Console.ForegroundColor = originalColor);
        }
    }
}