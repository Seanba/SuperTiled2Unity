using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    [CustomEditor(typeof(SuperColliderComponent))]
    class SuperColliderComponentEditor : UnityEditor.Editor
    {
        private static ST2USettings m_Settings;

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

            if (m_Settings == null)
            {
                m_Settings = ST2USettings.GetOrCreateST2USettings();

                if (m_Settings == null)
                {
                    // If something goes wrong use some dummy settings
                    m_Settings = CreateInstance<ST2USettings>();
                }
            }

            if (m_Settings != null)
            {
                if (component.m_PolygonShapes.Count > 0)
                {
                    ColliderGizmos.DrawColliderShapes(component, m_Settings);
                }
                else
                {
                    ColliderGizmos.DrawColliders(component.gameObject, m_Settings);
                }
            }
        }
    }
}
