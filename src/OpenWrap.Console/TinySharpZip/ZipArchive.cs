using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace TinySharpZip
{
    public enum CompressionMethod
    {
        Store = 0,
        Deflate = 8
    } ;

    public class ZipArchive
    {
        const string ARCHIVE_FORMAT_NOT_SUPPORTED_STRING = "Archive format is not supported.";
        const uint CENTRAL_FILE_HEADER_SIGNATURE = 0x02014b50;
        const ushort COMPRESSION_DEFLATE = 8;
        const ushort COMPRESSION_STORE = 0;
        const uint END_OF_CENTRAL_DIR_SIGNATURE = 0x06054b50;
        const uint LOCAL_FILE_HEADER_SIGNATURE = 0x04034b50;

        const int STREAM_EXCHANGE_BUFFER_SIZE = 0x1000;
        const ushort UTF8_FILE_NAME_ENCODING_FLAG = 1 << 11;
        const ushort VERSION_NEEDED_TO_EXTRACT = 0x0014;

        List<ZipEntry> entries;

        public ZipArchive()
        {
            entries = new List<ZipEntry>();
        }

        public List<ZipEntry> Entries
        {
            get { return entries; }
            set { entries = value; }
        }
        public static void Extract(Stream zipStream, string targetDirectory)
        {
            var zipArchive = new ZipArchive();
            zipArchive.ReadFrom(zipStream);
            foreach (var entry in zipArchive.Entries)
            {
                if (entry is ZipFileEntry)
                {
                    var fileEntry = entry as ZipFileEntry;
                    string absoluteFilePath = Path.Combine(targetDirectory, fileEntry.FileName);
                    string directoryName = Path.GetDirectoryName(absoluteFilePath);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    if (File.Exists(absoluteFilePath))
                    {
                        File.Delete(absoluteFilePath);
                    }
                    var fileStream = new FileStream(absoluteFilePath, FileMode.CreateNew);
                    if (fileEntry.Data != null && fileEntry.Data.Length != 0)
                    {
                        WriteStream(fileEntry.Data, fileStream);
                    }
                    fileStream.Close();
                }
                else if (entry is ZipDirectoryEntry)
                {
                    var directoryEntry = entry as ZipDirectoryEntry;
                    string directoryName = Path.Combine(targetDirectory, directoryEntry.DirectoryName);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                }
            }
        }

        public void ReadFrom(Stream zipStream)
        {
            while (zipStream.Position < zipStream.Length)
            {
                var reader = new BinaryReader(zipStream, Encoding.ASCII);
                uint localFileHeaderSignature = reader.ReadUInt32();
                if (localFileHeaderSignature != LOCAL_FILE_HEADER_SIGNATURE)
                {
                    // Local file entries are finished, next records are central directory records
                    break;
                }
                ushort versionNeededToExtract = reader.ReadUInt16();
                ushort generalPurposeBitFlag = reader.ReadUInt16();
                ushort compressionMethod = reader.ReadUInt16();
                if (compressionMethod != COMPRESSION_DEFLATE && compressionMethod != COMPRESSION_STORE)
                {
                    throw new NotSupportedException(ARCHIVE_FORMAT_NOT_SUPPORTED_STRING);
                }
                uint lastModifiedDateTime = reader.ReadUInt32();
                uint crc32 = reader.ReadUInt32();
                uint compressedSize = reader.ReadUInt32();
                uint uncompressedSize = reader.ReadUInt32();
                ushort fileNameLength = reader.ReadUInt16();
                ushort extraFieldLength = reader.ReadUInt16();
                if (extraFieldLength != 0)
                {
                    throw new NotSupportedException(ARCHIVE_FORMAT_NOT_SUPPORTED_STRING);
                }

                var fileNameBytes = reader.ReadBytes(fileNameLength);
                Encoding fileNameEncoding;
                if ((generalPurposeBitFlag & UTF8_FILE_NAME_ENCODING_FLAG) != 0)
                    fileNameEncoding = new UTF8Encoding();
                else
                    fileNameEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);

                string fileName = new string(fileNameEncoding.GetChars(fileNameBytes));

                // According to ZIP specification, ZIP archives generally 
                // contain forward slashes so make Windows file name
                fileName = fileName.Replace('/', Path.DirectorySeparatorChar);

                ZipEntry entry = null;
                if (uncompressedSize != 0)
                {
                    var fileData = reader.ReadBytes((int)compressedSize);
                    var fileDataStream = new MemoryStream(fileData);
                    var fileEntry = new ZipFileEntry(fileName, fileDataStream);
                    if (compressionMethod == COMPRESSION_DEFLATE)
                    {
                        fileDataStream.Position = 0;
                        var deflateStream = new DeflateStream(fileDataStream, CompressionMode.Decompress);

                        var uncompressedFileData = new byte[uncompressedSize];

                        deflateStream.Read(uncompressedFileData, 0, (int)uncompressedSize);
                        fileEntry.Data = new MemoryStream(uncompressedFileData);
                    }
                    entry = fileEntry;
                }
                else if (fileName.EndsWith(@"\"))
                {
                    entry = new ZipDirectoryEntry(fileName);
                }
                else
                {
                    entry = new ZipFileEntry(fileName, null);
                }
                entry.SetLastModifiedDateTime(lastModifiedDateTime);
                entries.Add(entry);
            }
        }

        static void WriteStream(Stream sourceStream, Stream targetStream)
        {
            sourceStream.Position = 0;
            var buffer = new byte[STREAM_EXCHANGE_BUFFER_SIZE];
            while (true)
            {
                int bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                targetStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}