using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity
{
    // A tile that just "looks" wrong and lets us know that the map or tileset was not imported correctly
    // fixit - stretch goal: Can this look like tv snow?
    public class SuperBadTile : SuperTile
    {
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.color = Color.red;
            tileData.sprite = m_Sprite;
            Debug.Log("fixit - getting tile data");
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            return base.GetTileAnimationData(position, tilemap, ref tileAnimationData);
        }
    }
}
