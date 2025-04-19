using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity
{
    // A list of a all the sprites that are used in a collection of tilesets
    public class TilesetSprites : ScriptableObject
    {
        public List<Sprite> m_Sprites = new();
    }
}
