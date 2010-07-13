using System;
using System.IO;

namespace TinySharpZip
{
    internal class Crc32
    {
        #region Private Variables

        private static readonly uint CRC32_POLYNOMIAL = 0xedb88320;
        private static readonly uint[] CRC32_TABLE;

        #endregion

        #region Constructors

        static Crc32()
        {
            CRC32_TABLE = new uint[0x100];

            for (uint byteIndex = 0; byteIndex < 0x100; byteIndex++)
            {
                uint crcTableItem = byteIndex;
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    if ((crcTableItem & 1) != 0)
                    {
                        crcTableItem = CRC32_POLYNOMIAL ^ (crcTableItem >> 1);
                    }
                    else
                    {
                        crcTableItem = crcTableItem >> 1;
                    }
                }
                CRC32_TABLE[byteIndex] = crcTableItem;
            }
        }

        #endregion

        #region Public Methods

        internal static uint Compute(Stream bytes)
        {
            uint crc32Value = uint.MaxValue;
            int sourceByte = 0;
            while ((sourceByte = bytes.ReadByte()) != -1)
            {
                uint indexInTable = (crc32Value ^ (byte)sourceByte) & 0xFF;
                crc32Value = CRC32_TABLE[indexInTable] ^ (crc32Value >> 8);

            }
            crc32Value = ~crc32Value;
            return crc32Value;
        }

        #endregion
    }
}
