using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity
{
    public class TilesetSpriteData : ScriptableObject
    {
        public static readonly string AssetObjectName = $"_{nameof(TilesetSpriteData)}";

        public List<SpriteRectsPerTexture> m_SpriteRectsPerTextures = new List<SpriteRectsPerTexture>();

        // fixit - keep track of textures that must be re-imported. Are these texture assets, GUIDs, or assetpaths?
    }
}
