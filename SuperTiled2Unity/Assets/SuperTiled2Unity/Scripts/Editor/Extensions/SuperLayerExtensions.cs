using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    public static class SuperLayerExtensions
    {
        public static void SetWorldPosition(this SuperLayer layer, SuperImportContext context)
        {
            // Accumlate positions up the tree
            Vector3 position_w = new Vector3();
            foreach (var parent in layer.gameObject.GetComponentsInParent<SuperLayer>())
            {
                position_w += (Vector3)context.MakePoint(parent.m_OffsetX, parent.m_OffsetY);
                position_w.z += parent.gameObject.GetSuperPropertyValueFloat(StringConstants.Unity_ZPosition, 0);
            }

            // Add an additional offset if our tileset is present. These coordinates have already been transformed.
            if (layer.GetComponent<Tilemap>() != null || layer.GetComponent<SuperTilesAsObjectsTilemap>() != null)
            {
                position_w += context.TilemapOffset;
            }

            layer.transform.position = position_w;
        }
    }
}
