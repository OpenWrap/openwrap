namespace OpenWrap.Commands.Cli
{
    public static class StringExtensions
    {
        public const char light = '░';
        public const char medium = '▒';
        public const char dark = '▓';
        public const char full = '█';

        public static string ToProgressBar(this int progress)
        {
            var leftChar = progress >= 100 ? full : dark;
            var backgroundChar = progress >= 100 ? dark : light;
            var progressChars = (progress >= 100 ? progress - 100 : progress) / 10;
            return new string(leftChar, progressChars) + new string(backgroundChar, 10 - progressChars);

        }
    }
}