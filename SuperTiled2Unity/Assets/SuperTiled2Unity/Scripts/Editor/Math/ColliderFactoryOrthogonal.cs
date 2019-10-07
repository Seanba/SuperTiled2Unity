using System;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class ColliderFactoryOrthogonal : ColliderFactory
    {
        public ColliderFactoryOrthogonal(SuperImportContext importContext)
            : base(importContext)
        {
        }

        public override Vector2 TransformPoint(Vector2 point)
        {
            // No transform needed
            return point;
        }

        public override Collider2D MakeBox(GameObject go, float width, float height)
        {
            // In orthogonal space, a box is a box and the only transforms that are needed will put coordinates into Unity Space
            var collider = go.AddComponent<BoxCollider2D>();

            if (width == 0 && height == 0)
            {
                // By default Tiled gives rectangles a size of 10x10 if both values are zero
                // (No offset needed in this case)
                collider.size = ImportContext.MakeSize(10, 10);
            }
            else
            {
                // Use the offset because box collider is centered
                collider.offset = ImportContext.MakePoint(width * 0.5f, height * 0.5f);
                collider.size = ImportContext.MakeSize(width, height);
            }

            return collider;
        }

        public override Collider2D MakeEllipse(GameObject go, float width, float height)
        {
            if (width == height)
            {
                if (width == 0)
                {
                    // Default to a size of 20 with no offset
                    width = height = 20.0f;
                    var collider = go.AddComponent<CircleCollider2D>();
                    collider.radius = ImportContext.MakeSize(width, height).x * 0.5f;
                    return collider;
                }
                else
                {
                    // Use a circle collider
                    // Offset the collider by half radius due to upper-left corner of origin in Tiled Editor
                    var collider = go.AddComponent<CircleCollider2D>();
                    collider.offset = ImportContext.MakePoint(width * 0.5f, height * 0.5f);
                    collider.radius = ImportContext.MakeSize(width, height).x * 0.5f;

                    return collider;
                }
            }
            else
            {
                // Estimate an ellipse through a polygon collider
                int count = ImportContext.Settings.EdgesPerEllipse;
                float theta = ((float)Math.PI * 2.0f) / count;

                Vector2[] points = new Vector2[count];
                for (int i = 0; i < count; i++)
                {
                    points[i].x = width * 0.5f * (float)Math.Cos(theta * i);
                    points[i].y = height * 0.5f * (float)Math.Sin(theta * i);
                }

                var collider = go.AddComponent<PolygonCollider2D>();
                collider.offset = ImportContext.MakePoint(width * 0.5f, height * 0.5f);
                collider.SetPath(0, ImportContext.MakePoints(points));

                return collider;
            }
        }

        public override Collider2D MakePolygon(GameObject go, Vector2[] points)
        {
            points = ImportContext.MakePoints(points);
            var collider = go.AddComponent<PolygonCollider2D>();
            collider.SetPath(0, points);
            return collider;
        }

        public override Collider2D MakePolyline(GameObject go, Vector2[] points)
        {
            points = ImportContext.MakePoints(points);
            var collider = go.AddComponent<EdgeCollider2D>();
            collider.points = points;
            return collider;
        }
    }
}
