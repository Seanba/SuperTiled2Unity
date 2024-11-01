using System.Collections.Generic;

namespace SuperTiled2Unity.Editor
{
    public class GlobalTileDatabase
    {
        private readonly HashSet<int> m_IgnorableTileIds = new HashSet<int>();
        private readonly Dictionary<int, SuperTile> m_Tiles = new Dictionary<int, SuperTile>();

        public void RegisterTileset(int firstId, SuperTileset tileset)
        {
            foreach (var tile in tileset.m_Tiles)
            {
                m_Tiles[firstId + tile.m_TileId] = tile;
            }
        }

        public void RegisterIgnorableTile(int tileId)
        {
            m_IgnorableTileIds.Add(tileId);
        }

        public bool IsIgnorableTileId(int tileId)
        {
            return m_IgnorableTileIds.Contains(tileId);
        }

        public bool TryGetTile(int tileId, out SuperTile tile)
        {
            return m_Tiles.TryGetValue(tileId, out tile);
        }
    }
}
