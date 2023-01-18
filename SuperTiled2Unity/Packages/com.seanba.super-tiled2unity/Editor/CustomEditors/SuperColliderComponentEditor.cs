using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    [CustomEditor(typeof(SuperColliderComponent))]
    class SuperColliderComponentEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void DrawHandles(SuperColliderComponent component, GizmoType gizmoType)
        {
            // Only draw gizmos for the SuperObject if we have it in our set
            // This way users can disable gizmos for the SuperObject through the Gizmos menu
            if (!SuperColliderComponent.GizmoDrawCommands.Contains(component))
            {
                return;
            }
            else
            {
                SuperColliderComponent.GizmoDrawCommands.Remove(component);
            }

            if (component.m_PolygonShapes.Count > 0)
            {
                ColliderGizmos.DrawColliderShapes(component);
            }
            else
            {
                ColliderGizmos.DrawColliders(component.gameObject);
            }
        }
    }
}
