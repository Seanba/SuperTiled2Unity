using System;
using UnityEngine;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AseUserDataChunk : AseChunk
    {
        [Flags]
        public enum UserDataFlags : uint
        {
            HasText = 1,
            HasColor = 2,
            HasProperties = 4,
        }


        public override ChunkType ChunkType => ChunkType.UserData;

        public UserDataFlags Flags { get; }
        public string Text { get; }
        public byte[] ColorRGBA { get; }

        public AseUserDataChunk(AseFrame frame, AseReader reader)
            : base(frame)
        {
            Flags = (UserDataFlags)reader.ReadDWORD();
            
            if (Flags.HasFlag(UserDataFlags.HasText))
            {
                Text = reader.ReadSTRING();
            }

            if (Flags.HasFlag(UserDataFlags.HasColor))
            {
                ColorRGBA = reader.ReadBYTEs(4);
            }

            if (Flags.HasFlag(UserDataFlags.HasProperties))
            {
                var sizeInBytes = reader.ReadDWORD();
                var numberOfProperties = reader.ReadDWORD();
                Debug.LogError($"User properties are currently not handled. Size = {sizeInBytes}, Number of Properties = {numberOfProperties}");

                // Read the amount of data needed for this section so at least we can keep on reading other chunks
                int extra = (int)sizeInBytes - 8;
                if (extra >= 0)
                {
                    reader.ReadBYTEs(extra);
                }
            }

            // Place the user data in the last chunk
            if (reader.LastChunk != null)
            {
                reader.LastChunk.UserDataText = Text;
                reader.LastChunk.UserDataColor = ColorRGBA;
            }
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitUserDataChunk(this);
        }
    }
}
