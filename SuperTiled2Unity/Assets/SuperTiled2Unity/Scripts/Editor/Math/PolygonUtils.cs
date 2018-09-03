using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class PolygonUtils
    {
        // Group convex polygons into a composite collider
        public static void AddCompositePolygonCollider(GameObject go, List<Vector2[]> convexPolygons)
        {
            // If there is only one convex polygon then don't use a composite
            if (convexPolygons.Count == 1)
            {
                var polyCollider = go.AddComponent<PolygonCollider2D>();
                polyCollider.SetPath(0, convexPolygons[0]);
                polyCollider.gameObject.AddComponent<SuperColliderComponent>();
            }
            else
            {
                // Rigid body is needed for composite collider
                var rigid = go.AddComponent<Rigidbody2D>();
                rigid.bodyType = RigidbodyType2D.Static;
                rigid.simulated = true;

                // Colliders will be grouped by the composite
                // This way we have convex polygon paths (in the children) if needed
                // And we can have complex polygons represented by one object
                var composite = go.AddComponent<CompositeCollider2D>();
                composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
                composite.generationType = CompositeCollider2D.GenerationType.Manual;

                // Add polygon colliders
                foreach (var path in convexPolygons)
                {
                    var goPolygon = new GameObject("ConvexPolygon");
                    go.AddChildWithUniqueName(goPolygon);

                    var polyCollider = goPolygon.AddComponent<PolygonCollider2D>();
                    polyCollider.usedByComposite = true;
                    polyCollider.SetPath(0, path);

                    polyCollider.gameObject.AddComponent<SuperColliderComponent>();
                }

                composite.GenerateGeometry();
            }
        }

        // Postive value == CW, negative == CCW
        public static float SumOverEdges(Vector2[] points)
        {
            float sum = 0;

            for (int i = 0; i < points.Length; i++)
            {
                int j = (i + 1) % points.Length;
                float dx = points[j].x - points[i].x;
                float dy = points[j].y + points[i].y;

                sum += dx * dy;
            }

            return sum;
        }
    }
}
