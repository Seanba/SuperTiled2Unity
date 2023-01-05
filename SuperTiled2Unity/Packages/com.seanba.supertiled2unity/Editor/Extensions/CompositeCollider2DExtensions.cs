using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class CompositeCollider2DExtensions
    {
        public static void ST2UGeneratePolygonGeometry(this CompositeCollider2D composite)
        {
            var go = composite.gameObject;
            composite.GenerateGeometry();

            if (composite.geometryType == CompositeCollider2D.GeometryType.Polygons)
            {
                ReplaceWithPolygonCollider2D(composite);
            }
            else
            {
                ReplaceWithEdgeCollider2Ds(composite);
            }

            // We no longer need the composite or rigid body
            Object.DestroyImmediate(composite);
            Object.DestroyImmediate(go.GetComponent<Rigidbody2D>());

            // We no longer need composite polygon children
            var childPolygons = go.GetComponentsInChildren<PolygonCollider2D>();
            foreach (var poly in childPolygons)
            {
                if (poly.usedByComposite == true)
                {
                    Object.DestroyImmediate(poly.gameObject);
                }
            }
        }

        public static Vector2[] GetPathPoints(this CompositeCollider2D composite, int index)
        {
            int count = composite.GetPathPointCount(index);
            Vector2[] points = new Vector2[count];
            composite.GetPath(index, points);
            return points;
        }

        private static void ReplaceWithPolygonCollider2D(CompositeCollider2D composite)
        {
            var go = composite.gameObject;

            // The convex shape parts are put into our specialized collider component for debug drawing
            var super = go.AddComponent<SuperColliderComponent>();

            var combined = go.AddComponent<PolygonCollider2D>();
            combined.pathCount = composite.pathCount;
            combined.isTrigger = composite.isTrigger;

            // Copy composite paths to combined PolygonCollider2D
            for (int p = 0; p < composite.pathCount; p++)
            {
                var points = composite.GetPathPoints(p);

                if (points.Length > 0)
                {
                    combined.SetPath(p, points);
                }
            }

            // Switch the composite collider to outlines in order to get the outline of our convex with holes polygon
            if (composite.geometryType != CompositeCollider2D.GeometryType.Outlines)
            {
                composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
                composite.GenerateGeometry();
            }

            // Add outline shapes
            for (int p = 0; p < composite.pathCount; p++)
            {
                var points = composite.GetPathPoints(p);

                if (points.Length != 0)
                {
                    super.AddOutline(points);
                }
            }

            // Add shapes from polygon children (which are guaranteed to be convex)
            var childPolygons = go.GetComponentsInChildren<PolygonCollider2D>();
            foreach (var poly in childPolygons)
            {
                if (poly.usedByComposite == true)
                {
                    super.AddPolygonShape(poly.GetPath(0));
                }
            }
        }

        private static void ReplaceWithEdgeCollider2Ds(CompositeCollider2D composite)
        {
            // Add an edge collider child for every path in our composite
            var go = composite.gameObject;

            for (int p = 0; p < composite.pathCount; p++)
            {
                var points = composite.GetPathPoints(p).ToList();

                // Close the loop
                points.Add(points[0]);

                // Add the edge child
                var goEdge = new GameObject("Edge");
                go.AddChildWithUniqueName(goEdge);

                var edge = goEdge.AddComponent<EdgeCollider2D>();
                edge.isTrigger = composite.isTrigger;
                edge.points = points.ToArray();

                goEdge.AddComponent<SuperColliderComponent>();
            }
        }
    }
}
