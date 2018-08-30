using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SuperTiled2Unity.Editor.Geometry;

namespace SuperTiled2Unity.Editor
{
    public static class CollisionObjectExtensions
    {
        public static void AddCollider(this CollisionObject collision, SuperTile tile, GameObject goParent, SuperImportContext importContext)
        {
            var go = new GameObject(collision.m_ObjectName);

            if (collision.CollisionShapeType == CollisionShapeType.Polygon)
            {
                AddPolygonCollider(go, collision, tile, importContext);
            }
            else if (collision.CollisionShapeType == CollisionShapeType.Polyline)
            {
                AddEdgeCollider(go, collision, tile, importContext);
            }
            else if (collision.CollisionShapeType == CollisionShapeType.Ellipse)
            {
                AddEllipseCollider(go, collision, tile, importContext);
            }
            else if (collision.CollisionShapeType == CollisionShapeType.Rectangle)
            {
                AddBoxCollider(go, collision, tile, importContext);
            }

            goParent.AddChildWithUniqueName(go);
        }

        private static void AddPolygonCollider(GameObject go, CollisionObject collision, SuperTile tile, SuperImportContext importContext)
        {
            // Note that polygons may need to be decomposed into convex parts
            var points = importContext.MakePoints(collision.Points);

            // Triangulate the polygon points
            var triangulator = new Triangulator();
            var triangles = triangulator.TriangulatePolygon(points);

            // Gather triangles into a collection of convex polygons
            var composition = new ComposeConvexPolygons();
            var convexPolygons = composition.Compose(triangles);

            PolygonUtils.AddCompositePolygonCollider(go, convexPolygons);

            // Position is from top-left corner
            float height = importContext.MakeScalar(tile.m_Height);
            go.transform.localPosition = new Vector3(0, height, 0);
        }

        private static void AddEdgeCollider(GameObject go, CollisionObject collision, SuperTile tile, SuperImportContext importContext)
        {
            var edge = go.AddComponent<EdgeCollider2D>();
            edge.points = importContext.MakePoints(collision.Points);

            float height = importContext.MakeScalar(tile.m_Height);
            go.transform.localPosition = new Vector3(0, height, 0);

            go.AddComponent<SuperColliderComponent>();
        }

        private static void AddEllipseCollider(GameObject go, CollisionObject collision, SuperTile tile, SuperImportContext importContext)
        {
            // Add a circle collider if width == height. Otherwise, we have to use am approximate polygon representation.
            if (collision.m_Size.x == collision.m_Size.y)
            {
                var cirlce = go.AddComponent<CircleCollider2D>();
                cirlce.offset = importContext.MakePoint(collision.m_Size) * 0.5f;
                cirlce.radius = importContext.MakeScalar(collision.m_Size.x) * 0.5f;

                float height = importContext.MakeScalar(tile.m_Height);
                var xpos = importContext.MakeScalar(collision.m_Position.x);
                var ypos = importContext.MakeScalar(collision.m_Position.y);
                go.transform.localPosition = new Vector3(xpos, height - ypos);
                go.transform.localEulerAngles = new Vector3(0, 0, importContext.MakeRotation(collision.m_Rotation));

                go.AddComponent<SuperColliderComponent>();
            }
            else
            {
                AddPolygonCollider(go, collision, tile, importContext);
            }
        }

        private static void AddBoxCollider(GameObject go, CollisionObject collision, SuperTile tile, SuperImportContext importContext)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.offset = importContext.MakePoint(collision.m_Size) * 0.5f;
            box.size = importContext.MakeSize(collision.m_Size);

            float height = importContext.MakeScalar(tile.m_Height);
            var xpos = importContext.MakeScalar(collision.m_Position.x);
            var ypos = importContext.MakeScalar(collision.m_Position.y);
            go.transform.localPosition = new Vector3(xpos, height - ypos);
            go.transform.localEulerAngles = new Vector3(0, 0, importContext.MakeRotation(collision.m_Rotation));

            go.AddComponent<SuperColliderComponent>();
        }
    }
}
