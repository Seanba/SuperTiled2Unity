using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity
{
    public class SuperMap : MonoBehaviour
    {
        [ReadOnly]
        public string m_Version;

        [ReadOnly]
        public string m_TiledVersion;

        [ReadOnly]
        public MapOrientation m_Orientation;

        [ReadOnly]
        public MapRenderOrder m_RenderOrder;

        [ReadOnly]
        public int m_Width;

        [ReadOnly]
        public int m_Height;

        [ReadOnly]
        public int m_TileWidth;

        [ReadOnly]
        public int m_TileHeight;

        [ReadOnly]
        public int m_HexSideLength;

        [ReadOnly]
        public StaggerAxis m_StaggerAxis;

        [ReadOnly]
        public StaggerIndex m_StaggerIndex;

        [ReadOnly]
        public bool m_Infinite;

        [ReadOnly]
        public Color m_BackgroundColor;

        [ReadOnly]
        public int m_NextObjectId;

        private void Start()
        {
            // This is a hack so that Unity does not falsely report prefab instance differences from our importer map
            // Look for where renderer.detectChunkCullingBounds is set to Manual in the importer code which is the other part of this hack
            foreach (var renderer in GetComponentsInChildren<TilemapRenderer>())
            {
                renderer.detectChunkCullingBounds = TilemapRenderer.DetectChunkCullingBounds.Auto;
            }
        }

        public Vector3Int TiledIndexToGridCell(int index, int offset_x, int offset_y, int stride)
        {
            int x = index % stride;
            int y = index / stride;
            x += offset_x;
            y += offset_y;

            var pos3 = TiledCellToGridCell(x, y);
            pos3.y = -pos3.y;

            return pos3;
        }

        private Vector3Int TiledCellToGridCell(int x, int y)
        {
            if (m_Orientation == MapOrientation.Isometric)
            {
                return new Vector3Int(-y, x, 0);
            }
            else if (m_Orientation == MapOrientation.Staggered)
            {
                var isStaggerX = m_StaggerAxis == StaggerAxis.X;
                var isStaggerOdd = m_StaggerIndex == StaggerIndex.Odd;

                if (isStaggerX)
                {
                    var pos = new Vector3Int(x - y, x + y, 0);

                    if (isStaggerOdd)
                    {
                        pos.x -= (x + 1) / 2;
                        pos.y -= x / 2;
                    }
                    else
                    {
                        pos.x -= x / 2;
                        pos.y -= (x + 1) / 2;
                    }

                    return pos;
                }
                else
                {
                    var pos = new Vector3Int(x, y + x, 0);

                    if (isStaggerOdd)
                    {
                        var stagger = y / 2;
                        pos.x -= stagger;
                        pos.y -= stagger;
                    }
                    else
                    {
                        var stagger = (y + 1) / 2;
                        pos.x -= stagger;
                        pos.y -= stagger;
                    }

                    return pos;
                }
            }
            else if (m_Orientation == MapOrientation.Hexagonal)
            {
                var isStaggerX = m_StaggerAxis == StaggerAxis.X;
                var isStaggerOdd = m_StaggerIndex == StaggerIndex.Odd;

                if (isStaggerX)
                {
                    // Flat top hex
                    if (isStaggerOdd)
                    {
                        return new Vector3Int(-y, -x - 1, 0);
                    }
                    else
                    {
                        return new Vector3Int(-y, -x, 0);
                    }
                }
                else
                {
                    // Pointy top hex
                    if (isStaggerOdd)
                    {
                        return new Vector3Int(x, y, 0);
                    }
                    else
                    {
                        return new Vector3Int(x, y + 1, 0);
                    }
                }
            }

            // Simple maps (like orthongal do not transform indices into other spaces)
            return new Vector3Int(x, y, 0);
        }
    }
}
