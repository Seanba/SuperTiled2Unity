﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // The collision builder for Tile Layers uses this class to combine colliders together
    public class TilePolygonCollection
    {
        private static readonly Matrix4x4 HorizontalFlipMatrix = MatrixUtils.Rotate2d(-1, 0, 0, 1);
        private static readonly Matrix4x4 VerticalFlipMatrix = MatrixUtils.Rotate2d(1, 0, 0, -1);
        private static readonly Matrix4x4 DiagonalFlipMatrix = MatrixUtils.Rotate2d(0, -1, -1, 0);

        private readonly SuperTile m_Tile;
        private readonly TileIdMath m_TileId;
        private readonly Matrix4x4 m_Transform;

        private List<TilePolygon> m_Polygons = new List<TilePolygon>();

        private SuperImportContext m_ImportContext;

        public TilePolygonCollection(SuperTile tile, TileIdMath tileId, SuperImportContext importContext, SuperMap mapComponent)
        {
            m_ImportContext = importContext;

            m_Tile = tile;
            m_TileId = tileId;

            m_Transform = m_Tile.GetTransformMatrix(m_TileId.FlipFlags, mapComponent);

            CollectTilePolygons();
        }

        public List<TilePolygon> Polygons { get { return m_Polygons; } }

        private void CollectTilePolygons()
        {
            if (!m_Tile.m_CollisionObjects.IsEmpty())
            {
                foreach (var collision in m_Tile.m_CollisionObjects)
                {
                    var tilePoly = new TilePolygon();
                    tilePoly.IsClosed = collision.IsClosed;
                    tilePoly.ColliderLayerName = collision.m_PhysicsLayer;
                    tilePoly.ColliderLayerId = LayerMask.NameToLayer(collision.m_PhysicsLayer);
                    tilePoly.IsTrigger = collision.m_IsTrigger;

                    var points = m_ImportContext.MakePointsPPU(collision.Points);
                    points = points.Select(pt => (Vector2)m_Transform.MultiplyPoint(pt)).ToArray();

                    // Make sure the polygon points order is still CCW. Otherwise clipper may subtract polygons from each other.
                    if (PolygonUtils.SumOverEdges(points) < 0)
                    {
                        points = points.Reverse().ToArray();
                    }

                    tilePoly.Points = points;

                    m_Polygons.Add(tilePoly);
                }
            }
        }
    }
}
