using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    public class GlobalTileDatabase
    {
        private Dictionary<int, SuperTile> m_Tiles = new Dictionary<int, SuperTile>();

        public void RegisterTileset(int firstId, SuperTileset tileset)
        {
            foreach (var tile in tileset.m_Tiles)
            {
                m_Tiles[firstId + tile.m_TileId] = tile;
            }
        }

        public bool TryGetTile(int tileId, out SuperTile tile)
        {
            return m_Tiles.TryGetValue(tileId, out tile);
        }
    }
}
