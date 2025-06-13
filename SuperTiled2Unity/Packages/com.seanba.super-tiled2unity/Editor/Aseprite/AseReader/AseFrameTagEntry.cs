namespace SuperTiled2Unity.Ase.Editor
{
    public class AseFrameTagEntry
    {
        public ushort FromFrame { get; }
        public ushort ToFrame { get; }
        public LoopAnimationDirection LoopAnimationDirection { get; }
        public byte[] ColorRGB { get; }
        public string Name { get; }
        public bool IsOneShot { get; }

        public AseFrameTagEntry(AseReader reader)
        {
            FromFrame = reader.ReadWORD();
            ToFrame = reader.ReadWORD();
            LoopAnimationDirection = (LoopAnimationDirection)reader.ReadBYTE();

            // Ignore next 8 bytes
            reader.ReadBYTEs(8);

            ColorRGB = reader.ReadBYTEs(3);

            // Ignore a byte
            reader.ReadBYTE();

            Name = reader.ReadSTRING();

            // "OneShot" loop hack
            if (Name.StartsWith("[") && Name.EndsWith("]"))
            {
                Name = Name.Remove(0, 1);
                Name = Name.Remove(Name.Length - 1, 1);
                IsOneShot = true;
            }
            else
            {
                IsOneShot = false;
            }
        }
    }
}
