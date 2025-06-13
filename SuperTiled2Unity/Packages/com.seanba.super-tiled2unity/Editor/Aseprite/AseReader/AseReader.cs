using System;
using System.IO;
using System.Text;

namespace SuperTiled2Unity.Ase.Editor
{
    // Reads the binary *.ase/*.aseprite format
    public class AseReader : IDisposable
    {
        private BinaryReader m_BinaryReader;

        public int Position { get; private set; }
        public ColorDepth ColorDepth { get; private set; }

        public AseChunk LastChunk { get; set; }

        public AseReader(string asePath)
        {
            m_BinaryReader = new BinaryReader(File.Open(asePath, FileMode.Open));
        }

        ~AseReader()
        {
            Dispose(false);
        }

        public byte ReadBYTE()
        {
            Position++;
            return m_BinaryReader.ReadByte();
        }

        public byte[] ReadBYTEs(int count)
        {
            Position += count;
            return m_BinaryReader.ReadBytes(count);
        }

        public ushort ReadWORD()
        {
            Position += 2;
            return m_BinaryReader.ReadUInt16();
        }

        public short ReadSHORT()
        {
            Position += 2;
            return m_BinaryReader.ReadInt16();
        }

        public uint ReadDWORD()
        {
            Position += 4;
            return m_BinaryReader.ReadUInt32();
        }

        public int ReadLONG()
        {
            Position += 4;
            return m_BinaryReader.ReadInt32();
        }

        public ColorDepth ReadColorDepth()
        {
            // Keep track of our color depth. It is used when reading pixels.
            ColorDepth = (ColorDepth)ReadWORD();
            return ColorDepth;
        }

        public string ReadSTRING()
        {
            var length = ReadWORD();
            var bytes = ReadBYTEs(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && m_BinaryReader != null)
            {
                m_BinaryReader.Dispose();
                m_BinaryReader = null;
            }
        }
    }
}
