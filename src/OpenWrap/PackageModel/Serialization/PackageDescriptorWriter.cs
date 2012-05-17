using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenFileSystem.IO;

namespace OpenWrap.PackageModel.Serialization
{
    public class PackageDescriptorWriter
    {
        public void Write(IEnumerable<IPackageDescriptorEntry> descriptor, Stream descriptorStream)
        {
            var streamWriter = new StreamWriter(descriptorStream, Encoding.UTF8);
            foreach (var line in descriptor)
                streamWriter.Write(line.Name.EqualsNoCase("Comment") ? "# {1}\r\n" : "{0}: {1}\r\n", line.Name, line.Value);
            
            streamWriter.Flush();
        }

    }
}