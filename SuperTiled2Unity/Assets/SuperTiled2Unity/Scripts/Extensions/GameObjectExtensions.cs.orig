using UnityEngine;

namespace SuperTiled2Unity
{
    public static class GameObjectExtensions
    {
        // Like GetComponentInParent but doesn't check current object
        public static T GetComponentInAncestor<T>(this MonoBehaviour mono) where T : MonoBehaviour
        {
            return GetComponentInAncestor<T>(mono.gameObject);
        }

        // Like GetComponentInParent but doesn't check current object
        public static T GetComponentInAncestor<T>(this GameObject go) where T : MonoBehaviour
        {
            if (go == null)
            {
                return null;
            }

            var parent = go.transform.parent;
            if (parent == null)
            {
                return null;
            }

            return parent.GetComponentInParent<T>();
        }

        public static bool TryGetCustomPropertySafe(this GameObject go, string name, out CustomProperty property)
        {
            property = null;

            if (go == null)
            {
                return false;
            }

            var customProperties = go.GetComponent<SuperCustomProperties>();
            if (customProperties == null)
            {
                return false;
            }

            return customProperties.TryGetCustomProperty(name, out property);
        }
    }
}
