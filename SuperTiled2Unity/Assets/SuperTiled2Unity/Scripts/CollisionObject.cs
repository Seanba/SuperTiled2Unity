using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    [Serializable]
    public class CollisionObject
    {
        public int m_ObjectId;

        public string m_ObjectName;

        public string m_ObjectType;

        public Vector2 m_Position;

        public Vector2 m_Size;

        public float m_Rotation;

        public List<CustomProperty> m_CustomProperties;

        public string m_PhysicsLayer;

        public bool m_IsTrigger;

        [SerializeField]
        private Vector2[] m_Points;
        public Vector2[] Points { get { return m_Points; } }

        [SerializeField]
        private bool m_IsClosed;
        public bool IsClosed { get { return m_IsClosed; } }

        [SerializeField]
        private CollisionShapeType m_CollisionShapeType;
        public CollisionShapeType CollisionShapeType { get { return m_CollisionShapeType; } }

        public void MakePointsFromRectangle()
        {
            m_CollisionShapeType = CollisionShapeType.Rectangle;
            m_IsClosed = true;

            // Make the points the give us a rectangle shape
            // Note: points are counter-clockwise
            m_Points = new Vector2[4];
            m_Points[0] = new Vector2(m_Position.x, m_Position.y);
            m_Points[1] = new Vector2(m_Position.x, m_Position.y + m_Size.y);
            m_Points[2] = new Vector2(m_Position.x + m_Size.x, m_Position.y + m_Size.y);
            m_Points[3] = new Vector2(m_Position.x + m_Size.x, m_Position.y);
            ApplyRotationToPoints();
        }

        public void MakePointsFromEllipse(int numEdges)
        {
            m_CollisionShapeType = CollisionShapeType.Ellipse;
            m_IsClosed = true;

            // Estimate the ellipse with a polygon
            float theta = ((float)Math.PI * 2.0f) / numEdges;
            float half_x = m_Size.x * 0.5f;
            float half_y = m_Size.y * 0.5f;

            m_Points = new Vector2[numEdges];
            for (int i = 0; i < numEdges; i++)
            {
                m_Points[i].x = (m_Position.x + half_x) + half_x * Mathf.Cos(theta * i);
                m_Points[i].y = (m_Position.y + half_y) + half_y * Mathf.Sin(theta * i);
            }

            ApplyRotationToPoints();
        }

        public void MakePointsFromPolygon(Vector2[] points)
        {
            m_CollisionShapeType = CollisionShapeType.Polygon;
            m_IsClosed = true;
            m_Points = points.Select(pt => pt + m_Position).ToArray();
            ApplyRotationToPoints();
        }

        public void MakePointsFromPolyline(Vector2[] points)
        {
            m_CollisionShapeType = CollisionShapeType.Polyline;
            m_IsClosed = false;
            m_Points = points.Select(pt => pt + m_Position).ToArray();
            ApplyRotationToPoints();
        }

        private void ApplyRotationToPoints()
        {
            if (m_Rotation != 0)
            {
                var rads = m_Rotation * Mathf.Deg2Rad;
                var cos = Mathf.Cos(rads);
                var sin = Mathf.Sin(rads);

                var transIn = Matrix4x4.Translate(-m_Position);
                var rotate = MatrixUtils.Rotate2d(cos, -sin, sin, cos);
                var transOut = Matrix4x4.Translate(m_Position);

                var matrix = transOut * rotate * transIn;

                m_Points = m_Points.Select(p => (Vector2)matrix.MultiplyPoint(p)).ToArray();
            }
        }
    }
}
