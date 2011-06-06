using System.Text;

namespace OpenWrap.Configuration
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendQuoted(this StringBuilder builder, string value)
        {
            var needEncoding = value.IndexOfAny(new[] { '=', '"', ';' }) != -1;
            return needEncoding
                       ? builder.Append("\"").Append(value.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\"")
                       : builder.Append(value);
        }
    }
}