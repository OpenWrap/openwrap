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
    public class PackageDescriptorReaderWriter
    {
        const int FILE_READ_RETRIES = 5;
        const int FILE_READ_RETRIES_WAIT = 20;

        // TODO: Read-retry should be part of an extension method that can be reused for reading the index in indexed folder repositories.



        public IPackageDescriptor Read(IFile filePath)
        {
            if (!filePath.Exists)
                return null;
            IOException ioException = null;
            int tries = 0;
            while (tries < FILE_READ_RETRIES)
            {
                try
                {
                    using (var fileStream = filePath.OpenRead())
                    {
                        var descriptor = new PackageDescriptorReader().Read(fileStream);
                        if (descriptor.Name == null)
                            descriptor.Name = PackageNameUtility.GetName(filePath.NameWithoutExtension);
                        return descriptor;
                    }
                }
                catch (InvalidPackageException ex)
                {
                    throw new InvalidPackageException(String.Format("Invalid package for file '{0}'.", filePath.Path), ex);
                }
                catch (IOException ex)
                {
                    ioException = ex;
                    tries++;
                    Thread.Sleep(FILE_READ_RETRIES_WAIT);
                    continue;
                }
            }
            throw ioException;
        }

        public void Write(IEnumerable<IPackageDescriptorEntry> descriptor, Stream descriptorStream)
        {
            var streamWriter = new StreamWriter(descriptorStream, Encoding.UTF8);
            foreach (var line in descriptor)
                streamWriter.Write(line.Name.EqualsNoCase("Comment") ? "# {1}\r\n" : "{0}: {1}\r\n", line.Name, line.Value);
            
            streamWriter.Flush();
        }

    }
}