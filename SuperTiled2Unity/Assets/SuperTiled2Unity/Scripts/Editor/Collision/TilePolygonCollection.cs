using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // The collision builder for Tile Layers uses this class to combine colliders together
    public class TilePolygonCollection
    {
        private static readonly Matrix4x4 HorizontalFlipMatrix = MatrixUtils.Rotate2d(-1, 0, 0, 1);
        private static readonly Matrix4x4 VerticalFlipMatrix = MatrixUtils.Rotate2d(1, 0, 0, -1);
        private static readonly Matrix4x4 DiagonalFlipMatrix = MatrixUtils.Rotate2d(0, -1, -1, 0);

        private SuperTile m_Tile;
        private TileIdMath m_TileId;
        private Matrix4x4 m_Transform;

        private List<TilePolygon> m_Polygons = new List<TilePolygon>();

        private SuperImportContext m_ImportContext;

        public TilePolygonCollection(SuperTile tile, TileIdMath tileId, SuperImportContext importContext)
        {
            m_ImportContext = importContext;

            m_Tile = tile;
            m_TileId = tileId;

            CalculateTransform();
            CollectTilePolygons();
        }

        public List<TilePolygon> Polygons { get { return m_Polygons; } }

        private void CalculateTransform()
        {
            Matrix4x4 matTileOffset = Matrix4x4.Translate(m_ImportContext.MakePoint(m_Tile.m_TileOffsetX, m_Tile.m_TileOffsetY));

            if (!m_TileId.HasFlip)
            {
                m_Transform = matTileOffset;
                return;
            }

            var tileCenter = m_ImportContext.MakeSize(m_Tile.m_Width, -m_Tile.m_Height) * 0.5f;

            Matrix4x4 matTransIn = Matrix4x4.identity;
            Matrix4x4 matFlip = Matrix4x4.identity;
            Matrix4x4 matTransOut = Matrix4x4.identity;

            // Go to the tile center
            matTransIn = Matrix4x4.Translate(-tileCenter);

            // Do the flips
            if (m_TileId.HasHorizontalFlip)
            {
                matFlip *= HorizontalFlipMatrix;
            }

            if (m_TileId.HasVerticalFlip)
            {
                matFlip *= VerticalFlipMatrix;
            }

            if (m_TileId.HasDiagonalFlip)
            {
                matFlip *= DiagonalFlipMatrix;
            }

            // Go out of the tile center
            if (!m_TileId.HasDiagonalFlip)
            {
                matTransOut = Matrix4x4.Translate(tileCenter);
            }
            else
            {
                float diff = m_ImportContext.MakeScalar(m_Tile.m_Height - m_Tile.m_Width) * 0.5f;
                tileCenter.x += diff;
                tileCenter.y -= diff;
                matTransOut = Matrix4x4.Translate(tileCenter);
            }

            // Put it all together
            Matrix4x4 mat = matTileOffset * matTransOut * matFlip * matTransIn;

            // Remember our transformation matrix
            m_Transform = mat;
        }

        private void CollectTilePolygons()
        {
            if (!m_Tile.m_CollisionObjects.IsEmpty())
            {
                foreach (var collision in m_Tile.m_CollisionObjects)
                {
                    var tilePoly = new TilePolygon();
                    tilePoly.IsClosed = collision.IsClosed;
                    tilePoly.ColliderLayerId = LayerMask.NameToLayer(collision.m_PhysicsLayer);
                    tilePoly.IsTrigger = collision.m_IsTrigger;

                    var points = m_ImportContext.MakePoints(collision.Points);
                    points = points.Select(pt => (Vector2)m_Transform.MultiplyPoint(pt)).ToArray();
                    tilePoly.Points = points;

                    m_Polygons.Add(tilePoly);
                }
            }
        }
    }
}