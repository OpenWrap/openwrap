//
// Mono.Security.BitConverterLE.cs
//  Like System.BitConverter but always little endian
//
// Author:
//   Bernie Solomon
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Mono.Security
{
    internal sealed class BitConverterLE
    {
        private BitConverterLE()
        {
        }

        internal static byte[] GetBytes(int value)
        {
            return BitConverter.IsLittleEndian 
                ? new[] { (byte)(value), (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) }
                : new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value) };
        }

        internal static ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort)(BitConverter.IsLittleEndian 
                ? value[startIndex + 1] << 8 | value[startIndex + 0]
                : value[startIndex] << 8 | value[startIndex + 1]);
        }

        internal static uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint)(BitConverter.IsLittleEndian
                              ? value[startIndex + 3] << 24 |
                                value[startIndex + 2] << 16 |
                                value[startIndex + 1] << 8 |
                                value[startIndex + 0]
                              : value[startIndex] << 24 |
                                value[startIndex + 1] << 16 |
                                value[startIndex + 2] << 8 |
                                value[startIndex + 3]);
        }
    }
}