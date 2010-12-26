#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System;
using System.IO;
using System.Text;

namespace OpenWrap.IO
{
    public static class StreamExtensions
    {
        public static long CopyTo(this Stream stream, Stream destinationStream)
        {
            var buffer = new byte[4096];
            int readCount = 0;
            long totalWritten = 0;
            while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalWritten += readCount;
                destinationStream.Write(buffer, 0, readCount);
            }

            return totalWritten;
        }

        public static string ReadString(this Stream stream)
        {
            return ReadString(stream, Encoding.UTF8);
        }

        public static string ReadString(this Stream stream, Encoding encoding)
        {
            var stringReader = new StreamReader(stream, encoding);
            return stringReader.ReadToEnd();
        }

        public static byte[] ReadToEnd(this Stream stream)
        {
            var streamToReturn = stream as MemoryStream;
            if (streamToReturn == null)
            {
                streamToReturn = new MemoryStream();
                stream.CopyTo(streamToReturn);
                streamToReturn.Position = 0;
            }

            var destinationBytes = new byte[streamToReturn.Length - streamToReturn.Position];
            Buffer.BlockCopy(streamToReturn.GetBuffer(),
                             (int)streamToReturn.Position,
                             destinationBytes,
                             0,
                             (int)(streamToReturn.Length - streamToReturn.Position));
            return destinationBytes;
        }

        public static Func<Stream> ResetOnRead(this Stream stream)
        {
            return () =>
            {
                stream.Position = 0;
                return stream;
            };
        }
        public static StreamReader StreamReader(this Stream stream)
        {
            return StreamReader(stream, Encoding.UTF8);
        }

        public static StreamReader StreamReader(this Stream stream, Encoding encoding)
        {
            return new StreamReader(stream, encoding);
        }


        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void WriteString(this Stream stream, string text, Encoding encoding)
        {
            var bytes = encoding.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteString(this Stream stream, string text)
        {
            WriteString(stream, text, Encoding.UTF8);
        }


    }
}

#region Full license
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion