using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class Collider2DExtensions
    {
        public static void SetMergeWithComposite(this Collider2D collider2d, bool merge)
        {
#if UNITY_2023_1_OR_NEWER
            if (merge && !collider2d.compositeCapable)
            {
                Debug.LogWarning($"Super Tiled2Unity warning: Collider2D '{collider2d.name}' is not composite capable");
            }
            collider2d.compositeOperation = merge ? Collider2D.CompositeOperation.Merge : Collider2D.CompositeOperation.None;
#else
            collider2d.usedByComposite = merge;
#endif
        }

        public static bool GetMergeWithComposite(this Collider2D collider2d)
        {
#if UNITY_2023_1_OR_NEWER
            return collider2d.compositeOperation == Collider2D.CompositeOperation.Merge;
#else
            return collider2d.usedByComposite;
#endif
        }
    }
}
