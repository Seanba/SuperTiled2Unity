using System;
using System.Collections.Generic;
using System.Linq;
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

        private void Start()
        {
            // This is a bad hack but the CompositeCollider2D is currently broken in Unity
            // By putting this here we are (more) confident that the collision geometry is as-expected
            // Keep an eye on these links for when the real fix is ready:
            // https://github.com/Unity-Technologies/2d-extras/issues/34
            // https://fogbugz.unity3d.com/default.asp?1093506_ahe9u92nr8fojlcc
            gameObject.SetActive(false);
            gameObject.SetActive(true);

            foreach (var collider in GetComponentsInChildren<CompositeCollider2D>())
            {
                collider.GenerateGeometry();
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

        public Vector2 GetTileLayerOffset(float inversePPU)
        {
            return Vector2.zero; // fixit - should put offset on a grid object instead of seprate layers. Objects can be placed in global space.

            /*
            var offset = new Vector2(0, -m_TileHeight);

            if (m_Orientation == MapOrientation.Isometric)
            {
                offset.x = -m_TileWidth * 0.5f;
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
                        offset.x = 0;
                        offset.y = -m_TileWidth;
                    }
                    else
                    {
                        // fixit
                    }
                }
                else
                {
                    // Pointy top hex
                    if (isStaggerOdd)
                    {
                        // fixit
                    }
                    else
                    {
                        // fixit
                    }
                }
            }

            //else if (m_Orientation == MapOrientation.Hexagonal && m_StaggerAxis == StaggerAxis.Y && m_StaggerIndex == StaggerIndex.Even)
            //{
            //    // Offset by one half of one half a tile height
            //    offset.y = -m_TileHeight * 0.25f;
            //}

            return offset * inversePPU;
            */
        }


        private Vector3Int TiledCellToGridCell(int x, int y)
        {
            //return new Vector3Int(x, y, 0);

            if (m_Orientation == MapOrientation.Isometric) // fixit
            {
                return new Vector3Int(-y, x, 0);
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
