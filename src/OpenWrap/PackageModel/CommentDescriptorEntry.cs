using System;
using System.IO;

namespace OpenWrap.PackageModel
{
    public class CommentDescriptorEntry : IPackageDescriptorEntry
    {
        public CommentDescriptorEntry(string value)
        {
            Value = value;
        }

        public string Name
        {
            get { return "COMMENT"; }
        }

        public string Value { get; private set; }

        public void Write(TextWriter writer)
        {
            writer.Write("{0}\r\n", Value);
        }
    }
}