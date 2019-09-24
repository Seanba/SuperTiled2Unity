using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class CompositeCollider2DExtensions
    {
        public static void ST2UGenerateGeometry(this CompositeCollider2D collider)
        {
            collider.GenerateGeometry();
            collider.generationType = CompositeCollider2D.GenerationType.Synchronous;
        }
    }
}
