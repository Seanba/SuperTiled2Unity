using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity
{
    public enum MapRenderOrder
    {
        Right_Down,
        Right_Up,
        Left_Down,
        Left_Up,
    }

    public static class MapRenderConverter
    {
        public static TilemapRenderer.SortOrder Tiled2Unity(MapRenderOrder order)
        {
            // Swap top/bottom when going from Tiled data to Unity
            switch (order)
            {
                case MapRenderOrder.Left_Down:
                    return TilemapRenderer.SortOrder.TopLeft;

                case MapRenderOrder.Left_Up:
                    return TilemapRenderer.SortOrder.BottomLeft;

                case MapRenderOrder.Right_Down:
                    return TilemapRenderer.SortOrder.TopRight;

                case MapRenderOrder.Right_Up:
                    return TilemapRenderer.SortOrder.BottomRight;
            }

            // Top left is a good default
            return TilemapRenderer.SortOrder.TopLeft;
        }
    }
}

