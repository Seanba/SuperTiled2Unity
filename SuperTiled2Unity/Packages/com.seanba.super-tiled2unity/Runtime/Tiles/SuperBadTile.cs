using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity
{
    // A tile that "looks wrong", indicationg that the map or tileset was not imported correctly
    public class SuperBadTile : SuperTile
    {
        public Color m_Color = Color.white;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.flags = TileFlags.LockColor;
            tileData.color = m_Color;
            tileData.sprite = m_Sprite;
        }
    }
}
