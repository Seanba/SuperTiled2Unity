using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

        public Vector3 CellSize { get; set; }

        public Vector3Int TiledIndexToGridCell(int index, int stride)
        {
            int x = index % stride;
            int y = index / stride;

            // Always off by one because tile positions start at bottom of the cell in Tiled
            y += 1;
            var pos3 = TiledCellToGridCell(x, y);
            pos3.y = -pos3.y;

            return pos3;
        }

        private Vector3Int TiledCellToGridCell(int x, int y)
        {
            // Orthogonal maps are easy
            Vector3Int grid = new Vector3Int(x, y, 0);

            if (m_Orientation == MapOrientation.Isometric)
            {
                grid = OrthoToIsometric(x, y);
            }
            else if (m_Orientation == MapOrientation.Staggered || m_Orientation == MapOrientation.Hexagonal)
            {
                grid = OrthoToStaggeredOrHexagonal(x, y);
            }
            else if (m_Orientation != MapOrientation.Orthogonal)
            {
                Debug.LogErrorFormat("Map orientation not supported for placing tiles: {0}", m_Orientation);
            }

            return grid;
        }

        public Vector3 CalculateCellSize()
        {
            if (m_Orientation == MapOrientation.Hexagonal)
            {
                var cell = new Vector3(m_TileWidth * 0.5f, m_TileHeight * 0.5f, 1);
                if (m_StaggerAxis == StaggerAxis.X)
                {
                    cell.x += m_HexSideLength * 0.5f;
                }
                else
                {
                    cell.y += m_HexSideLength * 0.5f;
                }

                return cell;
            }

            return new Vector3(m_TileWidth, m_TileHeight, 1);
        }

        private Vector3Int OrthoToIsometric(int x, int y)
        {
            var iso = new Vector3Int(-y, x, 0);
            return iso;
        }

        private Vector3Int OrthoToStaggeredOrHexagonal(int x, int y)
        {
            // This is simulated from Tiled Map Editor "tileToScreenCoords" method
            var point = new Vector3Int();

            // Round down to even number on tile width and height
            int tileWidth = m_TileWidth & ~1;
            int tileHeight = m_TileHeight & ~1;

            int sideLengthX = m_StaggerAxis == StaggerAxis.X ? m_HexSideLength : 0;
            int sideLengthY = m_StaggerAxis == StaggerAxis.Y ? m_HexSideLength : 0;

            int sideOffsetX = (tileWidth - sideLengthX) / 2;
            int sideOffsetY = (tileHeight - sideLengthY) / 2;

            int columnWidth = sideOffsetX + sideLengthX;
            int rowHeight = sideOffsetY + sideLengthY;

            if (m_StaggerAxis == StaggerAxis.X)
            {
                point.y = y * (tileHeight + sideLengthY);
                if (DoStaggerX(x))
                {
                    point.y += rowHeight;
                }

                point.x = x * columnWidth;
            }
            else
            {
                point.x = x * (tileWidth + sideLengthX);
                if (DoStaggerY(y))
                {
                    point.x += columnWidth;
                }

                point.y = y * rowHeight;
            }

            // The point is now if full blown world coordinates but we want it in cell coordinates
            point.x /= (int)CellSize.x;
            point.y /= (int)CellSize.y;

            return point;
        }

        private bool DoStaggerX(int x)
        {
            int staggerX = (m_StaggerAxis == StaggerAxis.X) ? 1 : 0;
            int staggerEven = (m_StaggerIndex == StaggerIndex.Even) ? 1 : 0;

            return staggerX != 0 && ((x & 1) ^ staggerEven) != 0;
        }

        private bool DoStaggerY(int y)
        {
            int staggerX = (m_StaggerAxis == StaggerAxis.X) ? 1 : 0;
            int staggerEven = (m_StaggerIndex == StaggerIndex.Even) ? 1 : 0;

            return staggerX == 0 && ((y & 1) ^ staggerEven) != 0;
        }
    }
}
