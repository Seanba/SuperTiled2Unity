namespace SuperTiled2Unity.Ase.Editor
{
    public class AsePaletteEntry
    {
        public ushort Flags { get; }
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }
        public byte Alpha { get; }
        public string Name { get; }

        public bool HasName => (Flags & 0x0001) != 0;

        public AsePaletteEntry(AseReader reader)
        {
            Flags = reader.ReadWORD();
            Red = reader.ReadBYTE();
            Green = reader.ReadBYTE();
            Blue = reader.ReadBYTE();
            Alpha = reader.ReadBYTE();

            if (HasName)
            {
                Name = reader.ReadSTRING();
            }
        }
    }
}
