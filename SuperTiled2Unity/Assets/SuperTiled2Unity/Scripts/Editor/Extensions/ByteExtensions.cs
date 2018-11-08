using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using SuperTiled2Unity.Ionic.Zlib;

namespace SuperTiled2Unity.Editor
{
    public static class ByteExtensions
    {
        public static uint[] ToUInts(this byte[] bytes)
        {
            uint[] u32s = new uint[bytes.Length / 4];
            for (int i = 0; i < u32s.Length; ++i)
            {
                u32s[i] = BitConverter.ToUInt32(bytes, i * 4);
            }
            return u32s;
        }

        public static byte[] GzipDecompress(this byte[] bytesCompressed)
        {
            MemoryStream streamCompressed = new MemoryStream(bytesCompressed);

            // Now, decompress the bytes
            using (MemoryStream streamDecompressed = new MemoryStream())
            using (GZipStream deflateStream = new GZipStream(streamCompressed, Ionic.Zlib.CompressionMode.Decompress))
            {
                deflateStream.CopyTo(streamDecompressed);
                return streamDecompressed.ToArray();
            }
        }

        public static byte[] ZlibDeflate(this byte[] bytesCompressed)
        {
            MemoryStream streamCompressed = new MemoryStream(bytesCompressed);

            // Nasty trick: Have to read past the zlib stream header
            streamCompressed.ReadByte();
            streamCompressed.ReadByte();

            // Now, decompress the bytes
            using (MemoryStream streamDecompressed = new MemoryStream())
            using (DeflateStream deflateStream = new DeflateStream(streamCompressed, Ionic.Zlib.CompressionMode.Decompress))
            {
                deflateStream.CopyTo(streamDecompressed);
                return streamDecompressed.ToArray();
            }
        }
    }
}
