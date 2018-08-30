using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    // Helper class that goes on a gameobject that has a piece of collider geometry on it
    // Tile layers may have many of such children with their collider components gathered into one CompositeCollider2D
    public class SuperColliderComponent : MonoBehaviour
    {
        // Editor code to help us manage when we draw gizmo colliders for this component
#if UNITY_EDITOR
        public static HashSet<SuperColliderComponent> GizmoDrawCommands = new HashSet<SuperColliderComponent>();

        private void OnDrawGizmosSelected()
        {
            GizmoDrawCommands.Add(this);
        }
#endif
    }
}
