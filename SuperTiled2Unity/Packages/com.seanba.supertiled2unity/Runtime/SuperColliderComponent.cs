using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity
{
    // Helper class that goes on a gameobject that has a piece of collider geometry on it
    // Tile layers may have many of such children with their collider components gathered into one CompositeCollider2D
    public class SuperColliderComponent : MonoBehaviour
    {
        [Serializable]
        public class Shape
        {
            public Vector2[] m_Points;
        }

        // Editor code to help us manage when we draw gizmo colliders for this component
        public List<Shape> m_PolygonShapes = new List<Shape>();
        public List<Shape> m_OutlineShapes = new List<Shape>();

#if UNITY_EDITOR
        public void AddPolygonShape(IEnumerable<Vector2> points)
        {
            var shape = new Shape();
            shape.m_Points = points.ToArray();
            m_PolygonShapes.Add(shape);
        }

        public void AddOutline(IEnumerable<Vector2> points)
        {
            var shape = new Shape();
            shape.m_Points = points.ToArray();
            m_OutlineShapes.Add(shape);
        }

        public static HashSet<SuperColliderComponent> GizmoDrawCommands = new HashSet<SuperColliderComponent>();

        private void OnDrawGizmosSelected()
        {
            GizmoDrawCommands.Add(this);
        }
#endif
    }
}
