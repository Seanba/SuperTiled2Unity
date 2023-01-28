using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class ColliderGizmos
    {
        private static Texture2D LineTexture;
        private const float FillOpcacity = 0.25f;
        private const float LineThickness = 5.0f;

        public static void DrawColliderShapes(SuperColliderComponent collider)
        {
            foreach (var shape in collider.m_PolygonShapes)
            {
                FillShape(collider.gameObject, shape.m_Points);
            }

            foreach (var shape in collider.m_OutlineShapes)
            {
                OutlineShape(collider.gameObject, shape.m_Points);
            }
        }

        public static void DrawColliders(GameObject go)
        {
            foreach (var polygon in go.GetComponentsInChildren<PolygonCollider2D>())
            {
                DrawPolygon(polygon);
            }

            foreach (var box in go.GetComponentsInChildren<BoxCollider2D>())
            {
                DrawBox(box);
            }

            foreach (var circle in go.GetComponentsInChildren<CircleCollider2D>())
            {
                DrawCircle(circle);
            }

            foreach (var edge in go.GetComponentsInChildren<EdgeCollider2D>())
            {
                DrawLines(edge);
            }
        }

        private static void FillShape(GameObject go, Vector2[] points)
        {
            CheckHelpers();

            var transformed = points.Select(pt => go.transform.TransformPoint(pt)).ToArray();
            DrawAsConvexPolygonFill(go, transformed);
        }

        private static void OutlineShape(GameObject go, Vector2[] points)
        {
            CheckHelpers();

            var transformed = points.Select(pt => go.transform.TransformPoint(pt)).ToArray();
            DrawAsConvexPolygonOutline(go, transformed);
        }

        private static void DrawPolygon(PolygonCollider2D polygon)
        {
            CheckHelpers();

            // Note: we are assuming the PolygonCollider2D is convex when using this function
            Vector3 offset = polygon.transform.TransformVector(polygon.offset);
            var points = polygon.GetPath(0).Select(pt => polygon.transform.TransformPoint(pt) + offset).ToArray();
            DrawAsConvexPolygon(polygon.gameObject, points);
        }

        private static void DrawBox(BoxCollider2D box)
        {
            CheckHelpers();
            Vector3 offset = box.offset;

            var corners = new Vector2[]
            {
                new Vector2(box.offset.x - box.size.x * 0.5f, box.offset.y + box.size.y * 0.5f),
                new Vector2(box.offset.x + box.size.x * 0.5f, box.offset.y + box.size.y * 0.5f),
                new Vector2(box.offset.x + box.size.x * 0.5f, box.offset.y - box.size.y * 0.5f),
                new Vector2(box.offset.x - box.size.x * 0.5f, box.offset.y - box.size.y * 0.5f),
            };

            var points = corners.Select(pt => box.transform.TransformPoint(pt)).ToArray();
            DrawAsConvexPolygon(box.gameObject, points);
        }

        private static void DrawCircle(CircleCollider2D circle)
        {
            CheckHelpers();
            const int count = 50;
            float theta = ((float)Math.PI * 2.0f) / count;

            var radius = circle.radius;
            var offset = (Vector3)circle.offset;

            Vector3[] points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                points[i].x = offset.x + radius * (float)Math.Cos(theta * i);
                points[i].y = offset.y + radius * (float)Math.Sin(theta * i);
            }

            points = points.Select(pt => circle.transform.TransformPoint(pt)).ToArray();
            DrawAsConvexPolygon(circle.gameObject, points);
        }

        private static void DrawLines(EdgeCollider2D edge)
        {
            Vector3 offset = edge.transform.TransformVector(edge.offset);
            var points = edge.points.Select(pt => edge.transform.TransformPoint(pt) + offset).ToArray();

            Handles.color = GetColorFromObject(edge.gameObject);
            Handles.DrawAAPolyLine(LineTexture, LineThickness, points);
        }

        private static void CheckHelpers()
        {
            if (LineTexture == null)
            {
                LineTexture = new Texture2D(1, 2);
                LineTexture.SetPixel(0, 0, Color.white);
                LineTexture.SetPixel(0, 1, Color.white);
            }
        }

        private static void DrawAsConvexPolygonOutline(GameObject go, Vector3[] points)
        {
            var color = GetColorFromObject(go);
            Handles.color = color;
            Handles.DrawAAPolyLine(LineTexture, LineThickness, points);
            Handles.DrawAAPolyLine(LineTexture, LineThickness, points[0], points[points.Length - 1]);
        }

        private static void DrawAsConvexPolygonFill(GameObject go, Vector3[] points)
        {
            var color = GetColorFromObject(go);
            color.a *= FillOpcacity;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(points);
        }

        private static void DrawAsConvexPolygon(GameObject go, Vector3[] points)
        {
            var color = GetColorFromObject(go);
            Handles.color = color;
            Handles.DrawAAPolyLine(LineTexture, LineThickness, points);
            Handles.DrawAAPolyLine(LineTexture, LineThickness, points[0], points[points.Length - 1]);

            color.a *= FillOpcacity;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(points);
        }

        private static Color GetColorFromObject(GameObject go)
        {
            // Tile layers go first
            var tileLayer = go.GetComponentInParent<SuperTileLayer>();
            if (tileLayer != null)
            {
                // Use the color of our object in the layer
                var color = ST2USettings.instance.m_LayerColors.ElementAtOrDefault(go.layer);
                color.a = tileLayer.CalculateOpacity();
                return color;
            }

            // Then comes tile objects
            var objectLayer = go.GetComponentInParent<SuperObjectLayer>();
            if (objectLayer != null)
            {
                var color = objectLayer.m_Color;

                // Type takes precedence for super objects
                var superObject = go.GetComponent<SuperObject>();
                if (superObject != null)
                {
                    CustomObjectType objectType;
                    if (ST2USettings.instance.m_CustomObjectTypes.TryGetCustomObjectType(superObject.m_Type, out objectType))
                    {
                        color = objectType.m_Color;
                    }
                }

                color.a = objectLayer.CalculateOpacity();
                return color;
            }

            return NamedColors.Gray;
        }
    }
}
