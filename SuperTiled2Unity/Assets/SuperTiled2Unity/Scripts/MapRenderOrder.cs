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
            // Swap top/bottom and right/left when going from Tiled data to Unity
            switch (order)
            {
                case MapRenderOrder.Left_Down:
                    return TilemapRenderer.SortOrder.TopRight;

                case MapRenderOrder.Left_Up:
                    return TilemapRenderer.SortOrder.BottomRight;

                case MapRenderOrder.Right_Down:
                    return TilemapRenderer.SortOrder.TopLeft;

                case MapRenderOrder.Right_Up:
                    return TilemapRenderer.SortOrder.BottomLeft;
            }

            // Top left is a good default
            return TilemapRenderer.SortOrder.TopLeft;
        }
    }
}
