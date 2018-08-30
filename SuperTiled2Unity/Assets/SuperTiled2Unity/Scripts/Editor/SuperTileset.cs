using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    public class SuperTileset : ScriptableObject
    {
        [ReadOnly]
        public int m_TileWidth;

        [ReadOnly]
        public int m_TileHeight;

        [ReadOnly]
        public int m_Spacing;

        [ReadOnly]
        public int m_Margin;

        [ReadOnly]
        public int m_TileCount;

        [ReadOnly]
        public int m_TileColumns;

        [ReadOnly]
        public Vector2 m_TileOffset;

        [ReadOnly]
        public bool m_IsInternal;

        [ReadOnly]
        public bool m_IsImageCollection;

        public List<CustomProperty> m_CustomProperties;

        [ReadOnly]
        public List<SuperTile> m_Tiles = new List<SuperTile>();

        public bool TryGetTile(int tileId, out SuperTile tile)
        {
            tile = m_Tiles.FirstOrDefault(t => t.m_TileId == tileId);
            return tile != null;
        }
    }
}
