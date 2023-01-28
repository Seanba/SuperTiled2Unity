using System;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class ColliderFactoryIsometric : ColliderFactory
    {
        private readonly float m_MapTileWidth;
        private readonly float m_MapTileHeight;

        public ColliderFactoryIsometric(float mapTileWidth, float mapTileHeight, SuperImportContext importContext)
            : base(importContext)
        {
            m_MapTileWidth = mapTileWidth;
            m_MapTileHeight = mapTileHeight;
        }

        public override Vector2 TransformPoint(Vector2 point)
        {
            var iso = point;
            iso.x = iso.x / m_MapTileHeight;
            iso.y = iso.y / m_MapTileHeight;

            var xf = new Vector2();
            xf.x = (iso.x - iso.y) * m_MapTileWidth * 0.5f;
            xf.y = (iso.x + iso.y) * m_MapTileHeight * 0.5f;

            return xf;
        }

        public override Collider2D MakeBox(GameObject go, float width, float height)
        {
            // In isometric space, a box is skewed and therefore represented by a polygon 
            var points = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(width, 0),
                new Vector2(width, height),
                new Vector2(0, height),
            };

            // Points are transformed to isometric space and then into Unity coordinates
            var transformed = points.Select(p => ImportContext.MakePoint(TransformPoint(p))).ToArray();

            var collider = go.AddComponent<PolygonCollider2D>();
            collider.SetPath(0, transformed);

            return collider;
        }

        public override Collider2D MakeEllipse(GameObject go, float width, float height)
        {
            // Ellipses are always approximated with polygons in isometric maps
            int count = ImportContext.EdgesPerEllipse;
            float theta = ((float)Math.PI * 2.0f) / count;

            Vector2[] points = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                points[i].x = width * 0.5f * (float)Math.Cos(theta * i);
                points[i].y = height * 0.5f * (float)Math.Sin(theta * i);
                points[i] = TransformPoint(points[i]);
            }

            // Create the polygon with the offset in the center
            var collider = go.AddComponent<PolygonCollider2D>();
            collider.offset = ImportContext.MakePoint(TransformPoint(width * 0.5f, height * 0.5f));
            collider.SetPath(0, ImportContext.MakePoints(points));

            return collider;
        }

        public override Collider2D MakePolygon(GameObject go, Vector2[] points)
        {
            var transformed = points.Select(p => TransformPoint(p)).ToArray();
            transformed = ImportContext.MakePoints(transformed);

            var collider = go.AddComponent<PolygonCollider2D>();
            collider.SetPath(0, transformed);
            return collider;
        }

        public override Collider2D MakePolyline(GameObject go, Vector2[] points)
        {
            var transformed = points.Select(p => TransformPoint(p)).ToArray();
            transformed = ImportContext.MakePoints(transformed);

            var collider = go.AddComponent<EdgeCollider2D>();
            collider.points = transformed;
            return collider;
        }
    }
}
