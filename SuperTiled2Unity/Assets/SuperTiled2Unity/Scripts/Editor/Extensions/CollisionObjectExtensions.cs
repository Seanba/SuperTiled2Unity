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

            // Additional settings on the collider that was just added
            var addedCollider = go.GetComponent<Collider2D>();
            if (addedCollider != null)
            {
                addedCollider.isTrigger = importContext.GetIsTriggerOverridable(collision.m_IsTrigger);
            }

            goParent.AddChildWithUniqueName(go);
        }

        private static void AddPolygonCollider(GameObject go, CollisionObject collision, SuperTile tile, SuperImportContext importContext)
        {
            // Note that polygons may need to be decomposed into convex parts
            var points = importContext.MakePointsPPU(collision.Points);

            // Triangulate the polygon points
            var triangulator = new Triangulator();
            var triangles = triangulator.TriangulatePolygon(points);

            // Gather triangles into a collection of convex polygons
            var composition = new ComposeConvexPolygons();
            var convexPolygons = composition.Compose(triangles);

            PolygonUtils.AddCompositePolygonCollider(go, convexPolygons, importContext);
        }

        private static void AddEdgeCollider(GameObject go, CollisionObject collision, SuperTile tile, SuperImportContext importContext)
        {
            var edge = go.AddComponent<EdgeCollider2D>();
            edge.points = importContext.MakePointsPPU(collision.Points);

            go.AddComponent<SuperColliderComponent>();
        }

        private static void AddEllipseCollider(GameObject go, CollisionObject collision, SuperTile tile, SuperImportContext importContext)
        {
            // Add a circle collider if width == height. Otherwise, we have to use am approximate polygon representation.
            if (collision.m_Size.x == collision.m_Size.y)
            {
                var cirlce = go.AddComponent<CircleCollider2D>();
                cirlce.offset = importContext.MakePointPPU(collision.m_Size.x, -collision.m_Size.y) * 0.5f;
                cirlce.radius = importContext.MakeScalar(collision.m_Size.x) * 0.5f;

                var xpos = importContext.MakeScalar(collision.m_Position.x);
                var ypos = importContext.MakeScalar(collision.m_Position.y);
                go.transform.localPosition = new Vector3(xpos, ypos);
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
            box.offset = importContext.MakePointPPU(collision.m_Size.x, -collision.m_Size.y) * 0.5f;
            box.size = importContext.MakeSize(collision.m_Size);

            var xpos = importContext.MakeScalar(collision.m_Position.x);
            var ypos = importContext.MakeScalar(collision.m_Position.y);

            go.transform.localPosition = new Vector3(xpos, ypos);
            go.transform.localEulerAngles = new Vector3(0, 0, importContext.MakeRotation(collision.m_Rotation));

            go.AddComponent<SuperColliderComponent>();
        }
    }
}
